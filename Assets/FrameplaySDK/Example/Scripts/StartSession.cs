using FrameplaySDK;
using UnityEngine;
using UnityEngine.Analytics;

public class StartSession : MonoBehaviour
{
    public Camera Camera;//active scene camera

    /// <summary>
    /// This data is used to serve more relevant ad content to the player, generating more revenue and improving the player's experience.
    /// This data will not be used to personally identify players
    /// More information @ https://docs.frameplay.gg/guide/data_collected/
    /// </summary>
    private int _playerAge = -1;
    private Gender _playerGender = Gender.Unknown;
    private SystemLanguage _playerLanguage = SystemLanguage.Unknown;
    private string _playerId = "";

    void Start()
    {
        if (!Frameplay.SessionStarted)
        {
            var player = new FrameplaySDK.General.Player(_playerAge, _playerGender, _playerLanguage, _playerId);

            Frameplay.StartSession(player, Camera);
        }
        else
        {
            Frameplay.RegisterCamera(Camera);
        }
    }
}