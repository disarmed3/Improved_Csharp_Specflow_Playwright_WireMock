# Improved_Specflow_Playwright_WireMock
Improved Credit Card Payment Test Automation with C#.  
This project automates the testing of a credit card payment process using SpecFlow for Behavior Driven Development (BDD), Playwright for browser automation, and WireMock for simulating the backend.

The main goal of this project is to automate the payment form flow using BDD. The test exercises the browser→backend→UI interaction end-to-end by serving a local test page and verifying the result rendered in the browser.

Overview

1. Browser Automation (Playwright):  
   Playwright launches a browser, navigates to the local payment page, and simulates user actions (filling card details and clicking the "PAY" button).

2. Local Test Page:  
   The test serves `TestPages/PaymentPage.html` at `http://localhost:5050/`. The page contains a small inline script that submits the form via `fetch()` to `http://localhost:3030/payment` and displays the response in the `#payment-result` element.

3. Mock Server (WireMock):  
   WireMock runs at `http://localhost:3030` and is configured to handle CORS preflight (`OPTIONS`) and `POST /payment`. The server returns a success body which the page displays.

4. Assertions (Playwright + WireMock):  
   The test verifies the UI by asserting that the page's `#payment-result` contains the expected success message. Optionally, the test also inspects WireMock's logs to confirm the request was received.

Project notes

- `TestPages/PaymentPage.html` is included in the project and copied to the test output directory so it can be served during test runs.
- `StepDefinitions/CreditCardSteps.cs` starts a minimal local static server (`HttpListener`) to serve the page and configures WireMock with CORS-aware stubs.

How to run

- Ensure Playwright browsers are installed in the environment running the tests: `playwright install`.
- Run the tests with your usual runner (e.g., `dotnet test`).
- If running in CI, consider setting `Headless = true` in the Playwright launch options.

Notes

- The local server listens on `http://localhost:5050/` and WireMock on port `3030`. If those ports are in use or restricted in your environment, update the ports in the test code and the HTML accordingly.
- On some environments (notably restricted Windows accounts), `HttpListener` may require URL ACL configuration or elevated permissions to listen on a prefix.

Packages

- SpecFlow
- Microsoft.Playwright.NUnit
- WireMock.Net
- NUnit.Framework
