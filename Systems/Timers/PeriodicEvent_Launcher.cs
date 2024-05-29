
public class PeriodicEvent_Launcher : BRemoteLauncher
{
    PeriodicEvent[] periodics;
    
    void Awake()
    {
        FillArrayAwake(ref periodics);
    }

    public void SetSeconds(float seconds)
    {
        if (this.IsActiveAndEnabled())
        {
            FillArrayUpdate(ref periodics);
            foreach (PeriodicEvent periodic in periodics)
                if (periodic != null) periodic.SetSeconds(seconds);
        }
    }
}
