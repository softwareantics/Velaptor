namespace VelaptorTesting.Scenes;

using System.Drawing;
using Velaptor;
using Velaptor.Factories;
using Velaptor.Input;
using Velaptor.Scene;
using Velaptor.UI;

public class TextBoxScene : SceneBase
{
    private TextBox textBox;
    private IAppInput<KeyboardState> keyboard;
    private IAppInput<MouseState> mouse;
    private KeyboardState curKeyState;
    private KeyboardState prevKeyState;
    private MouseState prevMouseState;
    private Label selectedText;
    private Label cursorIndex;

    public override void LoadContent()
    {
        this.textBox = new TextBox();
        this.textBox.Left = (int)(WindowSize.Width / 4f);
        this.textBox.Top = (int)(WindowSize.Height / 2f);

        this.keyboard = InputFactory.CreateKeyboard();
        this.mouse = InputFactory.CreateMouse();

        this.selectedText = new Label
        {
            Position = new Point(100, 100),
            Color = Color.White,
        };

        this.cursorIndex = new Label
        {
            Position = new Point(this.selectedText.Position.X + (int)(this.selectedText.Width), this.selectedText.Position.Y), Color = Color.White,
        };

        AddControl(this.textBox);
        AddControl(this.selectedText);
        // AddControl(this.cursorIndex);

        base.LoadContent();
    }

    public override void Update(FrameTime frameTime)
    {
        this.curKeyState = this.keyboard.GetState();
        var currMouseState = this.mouse.GetState();

        this.selectedText.Text = $"Selected Text: {this.textBox.SelectedText}";

        this.prevKeyState = this.curKeyState;
        this.prevMouseState = currMouseState;
        base.Update(frameTime);
    }

    public override void Render()
    {
        base.Render();
    }
}
