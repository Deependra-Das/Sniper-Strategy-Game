namespace SniperStrategyGame.Event
{
    public struct UpdateMissionInfoEvent
    {
        public readonly string MissionName;
        public readonly string MissionGoal;

        public UpdateMissionInfoEvent(string missionName, string missionGoal)
        {
            MissionName = missionName;
            MissionGoal = missionGoal;
        }
    }
}
