namespace FirebaseCore.DTOs
{
    public struct GameStateDto
    {
        public GameStates state;
    }
    
    public enum GameStates : long
    {
        Register,
        Game,
        End
    }
}