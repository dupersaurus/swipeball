/// <summary>
/// A script that receives ticks from the GameState, directly or from another ITicker
/// </summary>
public interface ITicker {

    /// <summary>
    /// A tick from the game
    /// </summary>
    /// <param name="delta">Seconds elapsed since last tick</param>
    void Tick(float delta);
}
