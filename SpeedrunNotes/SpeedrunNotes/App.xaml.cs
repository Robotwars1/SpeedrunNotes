namespace SpeedrunNotes;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

        // Gets the value of preferences
        // If preference == null, set default value
        // Value of the users set preference is the same before as after update
        Preferences.Default.Get("IP", "localhost");
        Preferences.Default.Get("Port", 16834);

        MainPage = new MainPage();
	}
}
