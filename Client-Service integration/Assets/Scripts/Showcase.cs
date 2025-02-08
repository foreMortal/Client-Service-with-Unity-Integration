using TMPro;
using UnityEngine;
using UnityClient;

public class Showcase : MonoBehaviour
{
    [SerializeField] private GameObject loginCanvas, pointsCanvas;
    [SerializeField] private TMP_InputField login, password;
    [SerializeField] private TMP_Text[] texts;
    UnityClient.UnityClient client;

    private float points;
    private uint id;

    private void Awake()
    {
        client = new();
        client.OpenConnection();
    }

    private async void OnDisable()
    {
        await client.CloseConnection();
    }

    public async void Register()
    {
        var m = await client.RegisterNewUser(login.text, password.text);
        print(m.ToString());
    }

    public void IncreasePoints()
    {
        points += 0.5f;
        texts[1].text = "Points: " + points;
    }

    public void SavePoints()
    {
        client.ChangeUserPoints(id, points);
    }

    public async void Login()
    {
        var u = await client.GetUser(login.text, password.text);

        points = u.Points;
        id = u.Id;

        texts[0].text = "ID: " + id;
        texts[1].text = "Points: " + points;
        texts[2].text = "Registration date: " + u.RegistrationDate.Day + "-" + u.RegistrationDate.Month + "-" + u.RegistrationDate.Year;
        texts[3].text = "Name: " + u.Name;

        loginCanvas.SetActive(false);
        pointsCanvas.SetActive(true);
    }
}
