namespace Server.Models.Entities;

public abstract record Card
{
    private string Text;

    protected Card(string text)
    {
        Text = text;
    }
}
