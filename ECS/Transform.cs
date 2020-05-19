using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FedoraEngine.ECS.Entities;

namespace FedoraEngine.ECS
{
	public sealed class Transform
	{
		private Transform _parent;
		private DirtyType _hierarchyDirty;

		private bool _localDirty;
		private bool _localPositionDirty;
		private bool _localScaleDirty;
		private bool _localRotationDirty;
		private bool _positionDirty;
		private bool _worldToLocalDirty;
		private bool _worldInverseDirty;

		private Matrix _localTransform;

		private Matrix _worldTransform = Matrix.Identity;
		private Matrix _worldToLocalTransform = Matrix.Identity;
		private Matrix _worldInverseTransform = Matrix.Identity;

		private Matrix _scaleMatrix;
		private Matrix _rotationMatrix;
		private Matrix _translationMatrix;

		private Vector2 _position;
		private Vector2 _localPosition;

		private Vector2 _scale;
		private Vector2 _localScale;

		private float _rotation;
		private float _localRotation;

		[Flags]
		private enum DirtyType
		{
			Clean,
			PositionDirty,
			ScaleDirty,
			RotationDirty,
		}

		public enum Component
		{
			Position,
			Scale,
			Rotation,
		}

		public readonly Entity Entity;

		public Transform Parent
		{
			get => _parent;
			set => SetParent(value);
		}

		public List<Transform> Children { get; private set; } = new List<Transform>();

		public int ChildCount => Children.Count;

		public Vector2 Position
		{
			get
			{
				UpdateTransform();
				if (_positionDirty)
				{
					if (Parent == null)
					{
						_position = _localPosition;
					}
					else
					{
						Parent.UpdateTransform();
						Vector2.Transform(ref _localPosition, ref Parent._worldTransform, out _position);
					}

					_positionDirty = false;
				}

				return _position;
			}
			set => SetPosition(value);
		}

		public Vector2 LocalPosition
		{
			get
			{
				UpdateTransform();
				return _localPosition;
			}
			set => SetLocalPosition(value);
		}

		public float Rotation
		{
			get
			{
				UpdateTransform();
				return _rotation;
			}
			set => SetRotation(value);
		}

		public float RotationDegrees
		{
			get => MathHelper.ToDegrees(_rotation);
			set => SetRotation(MathHelper.ToRadians(value));
		}

		public float LocalRotation
		{
			get
			{
				UpdateTransform();
				return _localRotation;
			}
			set => SetLocalRotation(value);
		}

		public float LocalRotationDegrees
		{
			get => MathHelper.ToDegrees(_localRotation);
			set => LocalRotation = MathHelper.ToRadians(value);
		}

		public Vector2 Scale
		{
			get
			{
				UpdateTransform();
				return _scale;
			}
			set => SetScale(value);
		}

		public Vector2 LocalScale
		{
			get
			{
				UpdateTransform();
				return _localScale;
			}
			set => SetLocalScale(value);
		}

		public Matrix WorldInverseTransform
		{
			get
			{
				UpdateTransform();
				if (_worldInverseDirty)
				{
					Matrix.Invert(ref _worldTransform, out _worldInverseTransform);
					_worldInverseDirty = false;
				}

				return _worldInverseTransform;
			}
		}

		public Matrix LocalToWorldTransform
		{
			get
			{
				UpdateTransform();
				return _worldTransform;
			}
		}

		public Matrix WorldToLocalTransform
		{
			get
			{
				if (_worldToLocalDirty)
				{
					if (Parent == null)
					{
						_worldToLocalTransform = Matrix.Identity;
					}
					else
					{
						Parent.UpdateTransform();
						Matrix.Invert(ref Parent._worldTransform, out _worldToLocalTransform);
					}

					_worldToLocalDirty = false;
				}

				return _worldToLocalTransform;
			}
		}

		public Transform(Entity entity)
		{
			Entity = entity;
			_scale = _localScale = Vector2.One;
		}

		public Entity GetChild(int index)
		{
			return Children[index].Entity;
		}

		public Entity FindChildByName(string name)
		{
			foreach (var child in Children)
			{
				if (child.Entity.Name == name)
					return child.Entity;
			}
			return null;
		}

		public Transform SetParent(Transform parent)
		{
			if (_parent == parent)
				return this;

			_parent.Children.Remove(this);

			parent.Children.Add(this);

			_parent = parent;
			SetDirty(DirtyType.PositionDirty);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetPosition(Vector2 position)
		{
			if (position == _position)
				return this;

			_position = position;
			if (Parent != null)
				LocalPosition = Vector2.Transform(_position, WorldToLocalTransform);
			else
				LocalPosition = position;

			_positionDirty = false;

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetPosition(float x, float y)
		{
			return SetPosition(new Vector2(x, y));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetLocalPosition(Vector2 localPosition)
		{
			if (localPosition == _localPosition)
				return this;

			_localPosition = localPosition;
			_localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
			SetDirty(DirtyType.PositionDirty);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetRotation(float radians)
		{
			_rotation = radians;
			if (Parent != null)
				LocalRotation = Parent.Rotation + radians;
			else
				LocalRotation = radians;

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetRotationDegrees(float degrees)
		{
			return SetRotation(MathHelper.ToRadians(degrees));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetLocalRotation(float radians)
		{
			_localRotation = radians;
			_localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
			SetDirty(DirtyType.RotationDirty);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetLocalRotationDegrees(float degrees)
		{
			return SetLocalRotation(MathHelper.ToRadians(degrees));
		}

		public void LookAt(Vector2 pos)
		{
			var sign = _position.X > pos.X ? -1 : 1;
			var vectorToAlignTo = Vector2.Normalize(_position - pos);
			Rotation = sign * (float)Math.Acos(Vector2.Dot(vectorToAlignTo, Vector2.UnitY));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetScale(Vector2 scale)
		{
			_scale = scale;
			if (Parent != null)
				LocalScale = scale / Parent._scale;
			else
				LocalScale = scale;

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetScale(float scale)
		{
			return SetScale(new Vector2(scale));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetLocalScale(Vector2 scale)
		{
			_localScale = scale;
			_localDirty = _positionDirty = _localScaleDirty = true;
			SetDirty(DirtyType.ScaleDirty);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Transform SetLocalScale(float scale)
		{
			return SetLocalScale(new Vector2(scale));
		}

		public void RoundPosition()
		{
			Position = new Vector2((float)Math.Round(_position.X), (float)Math.Round(_position.Y));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void UpdateTransform()
		{
			if (_hierarchyDirty != DirtyType.Clean)
			{
				if (Parent != null)
					Parent.UpdateTransform();

				if (_localDirty)
				{
					if (_localPositionDirty)
					{
						Matrix.CreateTranslation(_localPosition.X, _localPosition.Y, 0f, out _translationMatrix);
						_localPositionDirty = false;
					}

					if (_localRotationDirty)
					{
						Matrix.CreateRotationZ(_localRotation, out _rotationMatrix);
						_localRotationDirty = false;
					}

					if (_localScaleDirty)
					{
						Matrix.CreateScale(_localScale.X, _localScale.Y, 0f, out _scaleMatrix);
						_localScaleDirty = false;
					}

					Matrix.Multiply(ref _scaleMatrix, ref _rotationMatrix, out _localTransform);
					Matrix.Multiply(ref _localTransform, ref _translationMatrix, out _localTransform);

					if (Parent == null)
					{
						_worldTransform = _localTransform;
						_rotation = _localRotation;
						_scale = _localScale;
						_worldInverseDirty = true;
					}

					_localDirty = false;
				}

				if (Parent != null)
				{
					Matrix.Multiply(ref _localTransform, ref Parent._worldTransform, out _worldTransform);

					_rotation = _localRotation + Parent._rotation;
					_scale = Parent._scale * _localScale;
					_worldInverseDirty = true;
				}

				_worldToLocalDirty = true;
				_positionDirty = true;
				_hierarchyDirty = DirtyType.Clean;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetDirty(DirtyType dirtyFlagType)
		{
			if ((_hierarchyDirty & dirtyFlagType) == 0)
			{
				_hierarchyDirty |= dirtyFlagType;

				switch (dirtyFlagType)
				{
					case DirtyType.PositionDirty:
						Entity.OnTransformChanged(Component.Position);
						break;
					case DirtyType.RotationDirty:
						Entity.OnTransformChanged(Component.Rotation);
						break;
					case DirtyType.ScaleDirty:
						Entity.OnTransformChanged(Component.Scale);
						break;
				}

				for (var i = 0; i < Children.Count; i++)
					Children[i].SetDirty(dirtyFlagType);
			}
		}

		public void CopyFrom(Transform transform)
		{
			_position = transform.Position;
			_localPosition = transform._localPosition;
			_rotation = transform._rotation;
			_localRotation = transform._localRotation;
			_scale = transform._scale;
			_localScale = transform._localScale;

			SetDirty(DirtyType.PositionDirty);
			SetDirty(DirtyType.RotationDirty);
			SetDirty(DirtyType.ScaleDirty);
		}

		public override string ToString()
		{
			return string.Format(
				"[Transform: parent: {0}, position: {1}, rotation: {2}, scale: {3}, localPosition: {4}, localRotation: {5}, localScale: {6}]",
				Parent != null, Position, Rotation, Scale, LocalPosition, LocalRotation, LocalScale);
		}
	}
}
