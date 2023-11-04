public interface IChatty
{
    void StartConversation();
    void EndConversation();
    void Say(string message);
    void RespondToPlayerChoice(int choiceIndex);
    void TalkToAlive(IChatty otherNPC);
}
