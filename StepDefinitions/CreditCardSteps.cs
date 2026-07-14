using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using WireMock.Server;
using WireMock.Settings;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Net;
using System.IO;
using NUnit.Framework;

[Binding]
public class CreditCardSteps : PageTest
{
    private WireMockServer _server;
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private HttpListener _pageServer;

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

        // Initialize WireMock server (CORS-enabled stub + POST)
        _server = WireMockServer.Start(new WireMockServerSettings { Port = 3030 });

        _server
            .Given(Request.Create().WithPath("/payment").UsingOptions())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Access-Control-Allow-Origin", "*")
                .WithHeader("Access-Control-Allow-Methods", "POST, OPTIONS")
                .WithHeader("Access-Control-Allow-Headers", "Content-Type")
            );

        _server
            .Given(Request.Create().WithPath("/payment").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Access-Control-Allow-Origin", "*")
                .WithBody("Successful transaction")
            );

        // Start local static page server
        StartPageServer();
    }

    private void StartPageServer()
    {
        _pageServer = new HttpListener();
        _pageServer.Prefixes.Add("http://localhost:5050/");
        _pageServer.Start();
        _ = Task.Run(async () =>
        {
            while (_pageServer.IsListening)
            {
                try
                {
                    var ctx = await _pageServer.GetContextAsync();
                    var bytes = await File.ReadAllBytesAsync(
                        Path.Combine(AppContext.BaseDirectory, "TestPages", "PaymentPage.html"));
                    ctx.Response.ContentType = "text/html";
                    ctx.Response.ContentLength64 = bytes.Length;
                    await ctx.Response.OutputStream.WriteAsync(bytes);
                    ctx.Response.OutputStream.Close();
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
            }
        });
    }

    [Given(@"I navigate to the payment web page")]
    public async Task GivenINavigateToThePaymentWebPage()
    {
        // Navigate to the locally-served page
        await _page.GotoAsync("http://localhost:5050/");
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
        // clicking PAY triggers the page's fetch() to WireMock
        await _page.GetByRole(AriaRole.Button, new() { Name = "PAY" }).ClickAsync();
    }

    [Then(@"I get a successful payment message")]
    public async Task ThenIGetASuccessfulPaymentMessage()
    {
        // Verify the page shows the success message populated by the fetch response
        await Expect(_page.Locator("#payment-result"))
            .ToHaveTextAsync("Successful transaction");

        // Optionally verify WireMock received the POST from the browser
        var logs = _server.FindLogEntries(
            Request.Create().WithPath("/payment").UsingPost());
        Assert.That(logs, Has.Exactly(1).Items);
    }

    [AfterScenario]
    public async Task Cleanup()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();

        try
        {
            _pageServer?.Stop();
            _pageServer?.Close();
        }
        catch { }

        _server.Dispose();
    }
}
