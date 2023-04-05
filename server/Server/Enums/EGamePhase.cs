using System.Text.Json.Serialization;

namespace Server.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EGamePhase
{
    WaitingToStart,
    PickingAnswers,
    ShowingAnswers,
    PickingWinner
}
