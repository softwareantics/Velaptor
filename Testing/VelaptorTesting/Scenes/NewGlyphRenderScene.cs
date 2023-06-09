namespace VelaptorTesting.Scenes;

using System.Drawing;
using System.Numerics;
using Velaptor.Content.Fonts;
using Velaptor.Factories;
using Velaptor.Graphics.Renderers;
using Velaptor.Scene;

public class NewGlyphRenderScene : SceneBase
{
    private IFontRenderer fontRenderer;
    private IFont font;
    private Vector2 position;

    public override void LoadContent()
    {
        var rendererFactory = new RendererFactory();

        this.fontRenderer = rendererFactory.CreateFontRenderer();
        this.font = ContentLoader.LoadFont("TimesNewRoman-Regular", 12);
        this.position = new Vector2(WindowSize.Width / 2f, WindowSize.Height / 2f);

        base.LoadContent();
    }

    public override void Render()
    {
        var text = "CW";
        var glyphs = this.font.ToGlyphMetrics(text);

        var firstGlyphBounds = glyphs[0].GlyphBounds;
        firstGlyphBounds.Width = 0f;
        glyphs[0].GlyphBounds = firstGlyphBounds;

        var renderPos = this.position;

        this.fontRenderer.RenderBaseNEW(this.font, glyphs, (int)renderPos.X, (int)renderPos.Y, 1f, 0f, Color.White);

        // this.fontRenderer.Render(this.font, text, this.position, Color.White);

        base.Render();
    }
}
