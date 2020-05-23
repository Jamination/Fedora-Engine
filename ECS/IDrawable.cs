namespace FedoraEngine.ECS
{
    public interface IDrawable
    {
        float RenderLayer { get; set; }

        bool Sorting { get; set; }

        void Draw();
    }
}
