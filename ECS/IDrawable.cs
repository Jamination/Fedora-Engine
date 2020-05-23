namespace FedoraEngine.ECS
{
    public interface IDrawable
    {
        float RenderLayer { get; set; }

        void Draw();
    }
}
