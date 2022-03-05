namespace PM8MP.Rooms
{
    public interface IRoomView
    {
        bool WasFirstMaster { get; set; }
        CommandReceiver CreateCommandReceiver(byte playerId);
        MasterReceiver CreateMasterReceiver(CommandReceiver commandReceiver);
        void OnDisconnect();
    }
}