namespace Velaptor.UI;

/// <summary>
/// Represents the various types of events that can occur with a text box cursor.
/// </summary>
internal enum TextBoxEvent
{
    /// <summary>
    /// A character is being added to the text box.
    /// </summary>
    AddingCharacter,

    /// <summary>
    /// A character is being removed from the text box.
    /// </summary>
    RemovingSingleChar,

    /// <summary>
    /// One or more characters are selected to be removed.
    /// </summary>
    RemovingSelectedChars,

    /// <summary>
    /// The cursor is moving.
    /// </summary>
    MovingCursor,
}

internal enum MutateType
{
    PreMutate,
    PostMutate,
}
