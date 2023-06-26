namespace VelaptorTesting.Scenes;

using System;
using System.Drawing;
using System.Linq;
using Velaptor;
using Velaptor.Factories;
using Velaptor.Input;
using Velaptor.Scene;
using Velaptor.UI;

public class TextBoxScene : SceneBase
{
    private const int CtrlLeft = 10;
    private TextBox textBox;
    private IAppInput<KeyboardState> keyboard;
    private IAppInput<MouseState> mouse;
    private KeyboardState prevKeyState;
    private MouseState prevMouseState;
    private Label currentSetting;
    private Label instructions;
    private Label selectedText;
    private TextBoxSetting currTxtBoxSetting = TextBoxSetting.TextColor;
    private ColorComponent colorComponent = ColorComponent.Red;

    public override void LoadContent()
    {
        this.textBox = new TextBox
        {
            Left = (int)(WindowSize.Width / 4f),
            Top = (int)(WindowSize.Height / 2f),
            TextColor = Color.IndianRed,
        };

        this.keyboard = InputFactory.CreateKeyboard();
        this.mouse = InputFactory.CreateMouse();

        this.currentSetting = new Label
        {
            Color = Color.White,
        };

        this.instructions = new Label
        {
            Color = Color.White,
        };
        UpdateSettingAndInstructions();

        this.selectedText = new Label
        {
            Color = Color.White,
            Text = "Selected Text:",
        };
        this.selectedText.Position = new Point(this.selectedText.Left, 200);
        this.selectedText.Left = 10;

        AddControl(this.textBox);
        AddControl(this.selectedText);
        AddControl(this.instructions);
        AddControl(this.currentSetting);

        base.LoadContent();
    }

    public override void Update(FrameTime frameTime)
    {
        var curKeyState = this.keyboard.GetState();
        var currMouseState = this.mouse.GetState();

        this.selectedText.Text = $"Selected Text: {this.textBox.SelectedText}";
        this.selectedText.Left = 10;

        ChangeSetting();

        this.prevKeyState = curKeyState;
        this.prevMouseState = currMouseState;

        base.Update(frameTime);
    }

    private void ChangeSetting()
    {
        var curKeyState = this.keyboard.GetState();
        var curMouseState = this.mouse.GetState();

        var isTabKeyPressed = curKeyState.IsKeyDown(KeyCode.Tab) && this.prevKeyState.IsKeyUp(KeyCode.Tab);

        if (isTabKeyPressed)
        {
            this.currTxtBoxSetting = curKeyState.AnyShiftKeysDown()
                ? this.currTxtBoxSetting.Previous()
                : this.currTxtBoxSetting.Next();

            UpdateSettingAndInstructions();
        }

        switch (this.currTxtBoxSetting)
        {
            case TextBoxSetting.TextColor:
                HandleTextBoxTextColor(curMouseState);
                break;
            case TextBoxSetting.MoveTextBox:
                HandleTextBoxMovement(curKeyState, curMouseState);
                break;
            case TextBoxSetting.TextBoxWidth:
                HandleTextBoxSize(curMouseState);
                break;
            case TextBoxSetting.FontChange:
                HandleTextBoxFontChange(curMouseState);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleTextBoxTextColor(MouseState curMouseState)
    {
        var isLeftMouseButtonPressed = curMouseState.IsLeftButtonDown() && this.prevMouseState.IsLeftButtonUp();

        if (isLeftMouseButtonPressed)
        {
            this.colorComponent = this.colorComponent.Next();
            UpdateSettingAndInstructions();
        }
        else if (curMouseState.GetScrollDirection() == MouseScrollDirection.ScrollUp)
        {
            this.textBox.TextColor = this.colorComponent switch
            {
                ColorComponent.Red => this.textBox.TextColor.IncreaseRedBy(10),
                ColorComponent.Green => this.textBox.TextColor.IncreaseGreenBy(10),
                ColorComponent.Blue => this.textBox.TextColor.IncreaseBlueBy(10),
                _ => throw new ArgumentOutOfRangeException()
            };

            UpdateSettingAndInstructions();
        }
        else if (curMouseState.GetScrollDirection() == MouseScrollDirection.ScrollDown)
        {
            this.textBox.TextColor = this.colorComponent switch
            {
                ColorComponent.Red => this.textBox.TextColor.DecreaseRedBy(10),
                ColorComponent.Green => this.textBox.TextColor.DecreaseGreenBy(10),
                ColorComponent.Blue => this.textBox.TextColor.DecreaseBlueBy(10),
                _ => throw new ArgumentOutOfRangeException()
            };

            UpdateSettingAndInstructions();
        }
    }

    private void HandleTextBoxMovement(KeyboardState curKeyState, MouseState curMouseState)
    {
        var isLeftMouseButtonPressed = curMouseState.IsLeftButtonDown() && this.prevMouseState.IsLeftButtonUp();
        var isRightMouseButtonPressed = curMouseState.IsRightButtonDown() && this.prevMouseState.IsRightButtonUp();

        if (curKeyState.AnyShiftKeysDown())
        {
            if (isLeftMouseButtonPressed)
            {
                this.textBox.Position = this.textBox.Position with { Y = this.textBox.Position.Y - 25 };
            }
            else if (isRightMouseButtonPressed)
            {
                this.textBox.Position = this.textBox.Position with { Y = this.textBox.Position.Y + 25 };
            }
        }
        else
        {
            if (isLeftMouseButtonPressed)
            {
                this.textBox.Position = this.textBox.Position with { X = this.textBox.Position.X - 25 };
            }
            else if (isRightMouseButtonPressed)
            {
                this.textBox.Position = this.textBox.Position with { X = this.textBox.Position.X + 25 };
            }
        }
    }

    private void HandleTextBoxSize(MouseState curMouseState)
    {
        var isLeftMouseButtonPressed = curMouseState.IsLeftButtonDown() && this.prevMouseState.IsLeftButtonUp();
        var isRightMouseButtonPressed = curMouseState.IsRightButtonDown() && this.prevMouseState.IsRightButtonUp();

        if (isLeftMouseButtonPressed)
        {
            this.textBox.Width += 15;
        }
        else if (isRightMouseButtonPressed)
        {
            this.textBox.Width -= 15;
        }
    }

    private void HandleTextBoxFontChange(MouseState curMouseState)
    {
        var isLeftMouseButtonPressed = curMouseState.IsLeftButtonDown() && this.prevMouseState.IsLeftButtonUp();
        var isRightMouseButtonPressed = curMouseState.IsRightButtonDown() && this.prevMouseState.IsRightButtonUp();

        if (isLeftMouseButtonPressed)
        {
            this.textBox.FontSize += 2u;
        }
        else if (isRightMouseButtonPressed)
        {
            this.textBox.FontSize -= 2u;
        }
    }

    private void UpdateSettingAndInstructions()
    {
        var clrComp = this.colorComponent switch
        {
            ColorComponent.Red => "(Red)",
            ColorComponent.Green => "(Green)",
            ColorComponent.Blue => "(Blue)",
            _ => throw new ArgumentOutOfRangeException(),
        };

        switch (this.currTxtBoxSetting)
        {
            case TextBoxSetting.TextColor:
                this.currentSetting.Text = $"Text Color {clrComp}: {this.textBox.TextColor.R}, {this.textBox.TextColor.G}, {this.textBox.TextColor.B}";
                this.instructions.Text = "Press the left mouse button to cycle which color component to use.\nScroll the mouse wheel to change the color.";
                break;
            case TextBoxSetting.MoveTextBox:
                this.currentSetting.Text = "Text Box Movement";
                this.instructions.Text = "Left mouse button to move left and right mouse button to move right.";
                this.instructions.Text += "\nDo not hold shift to move horizontally and hold shift down to move vertically.";
                break;
            case TextBoxSetting.TextBoxWidth:
                this.currentSetting.Text = "Text Box Width";
                this.instructions.Text = "Left mouse button to increase size and right mouse button to decrease size.";
                break;
            case TextBoxSetting.FontChange:
                this.currentSetting.Text = "Font Change";
                this.instructions.Text = "Left mouse button to increase font size and right mouse button to decrease font size.";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var lines = this.instructions.Text.Split('\n');

        var maxLineWidth = lines.Max(l => this.instructions.Font.Measure(l).Width);
        this.currentSetting.Position = new Point(WindowCenter.X, 30);
        this.instructions.Left = WindowCenter.X - ((int)maxLineWidth / 2);
        this.instructions.Top = 60;
    }
}
