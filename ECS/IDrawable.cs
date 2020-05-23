namespace FedoraEngine.ECS
{
    public interface IDrawable
    {
        int RenderLayer { get; set; }

        void Draw();
    }
}
