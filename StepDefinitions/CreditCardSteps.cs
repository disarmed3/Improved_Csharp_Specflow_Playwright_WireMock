using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using WireMock.Server;
using WireMock.Settings;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Text;
using NUnit.Framework;

[Binding]
public class CreditCardSteps : PageTest
{
    private readonly HttpClient _client = new HttpClient();
    private HttpResponseMessage _response;
    private WireMockServer _server;
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;

    private record PaymentRequest
    {
        public string name { get; init; }
        public string cardNumber { get; init; }
        public string expiryDate { get; init; }
        public string cvc { get; init; }
    }

    private readonly PaymentRequest requestBody = new PaymentRequest
    {
        name = "George Papadopoulos",
        cardNumber = "1234567812345678",
        expiryDate = "12/25",
        cvc = "123"
    };

    [BeforeScenario]
    public async Task Setup()
    {
        // Initialize Playwright
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        _page = await _browser.NewPageAsync();

        // Initialize WireMock server
        _server = WireMockServer.Start(new WireMockServerSettings { Port = 3030 });
        _server
            .Given(Request.Create().WithPath("/payment").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithBody("Successful transaction")
            );
    }

    [Given(@"I navigate to the payment web page")]
    public async Task GivenINavigateToThePaymentWebPage()
    {
        await _page.GotoAsync("https://ovvwzkzry9.csb.app/");
        await _page.GetByRole(AriaRole.Link, new() { Name = "Yes, proceed to preview" }).ClickAsync();
    }

    [Then(@"I fill-in my valid credentials")]
    public async Task ThenIFill_InMyValidCredentials()
    {
        await _page.GetByPlaceholder("Card Number").FillAsync(requestBody.cardNumber);
        await _page.GetByPlaceholder("Name").FillAsync(requestBody.name);
        await _page.GetByPlaceholder("Valid Thru").FillAsync(requestBody.expiryDate);
        await _page.GetByPlaceholder("CVC").FillAsync(requestBody.cvc);
    }

    [When(@"I click the pay button")]
    public async Task WhenIClickThePayButton()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "PAY" }).ClickAsync();
        var jsonContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        _response = await _client.PostAsync("http://localhost:3030/payment", jsonContent);
    }

    [Then(@"I get a successful payment message")]
    public async Task ThenIGetASuccessfulPaymentMessage()
    {
        // Verify the HTTP response is not null
        Assert.That(_response, Is.Not.Null, "Response should not be null");

        // Verify the HTTP response status code
        Assert.That((int)_response.StatusCode, Is.EqualTo(201), "Expected status code 201");

        // Verify the response content
        var content = await _response.Content.ReadAsStringAsync();
        Assert.That(content, Is.EqualTo("Successful transaction"),
            "Expected response content to be 'Successful transaction'");
    }

    [AfterScenario]
    public async Task Cleanup()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
        _server.Dispose();
    }
}