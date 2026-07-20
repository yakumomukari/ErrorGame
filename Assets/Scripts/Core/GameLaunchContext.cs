public static class GameLaunchContext
{
    private static bool continueRequested;

    public static void RequestNewGame()
    {
        continueRequested = false;
    }

    public static void RequestContinue()
    {
        continueRequested = true;
    }

    public static bool ConsumeContinueRequest()
    {
        bool result = continueRequested;
        continueRequested = false;
        return result;
    }
}
