# Improved_Specflow_Playwright_WireMock
Improved Credit Card Payment Test Automation with C#.  
This project automates the testing of a credit card payment process using SpecFlow for Behavior Driven Development (BDD), Playwright for browser automation, and WireMock for simulating the backend server. The project is implemented using C# with NUnit as the test framework.  

The main goal of this project is to automate the testing of a credit card payment form using a BDD (Behavior Driven Development) approach. It tests the full flow of a payment process by:  

Steps Overview:
1. Browser Automation (Playwright):  
Playwright is used to launch a browser, navigate to the payment page, and simulate the user's actions (filling in card details, clicking the "PAY" button).  

2. Mock Server (WireMock):  
A mock server is initialized using WireMock to simulate the backend payment processing API at http://localhost:3030/payment.  
It responds to the POST requests made by the test with a successful payment message.  

3. POST Request (HttpClient):  
After simulating the button click, a POST request is made using HttpClient to the WireMock server to send the payment data (name, card number, expiry date, and CVC).  

4. Assertions (NUnit):  
Once the POST request is sent, the test asserts that the transaction is successful by checking the response status code (201 Created) and the response body ("Successful transaction").   

Packages:  
1. SpecFlow, 2.Microsoft.Playwright.NUnit, 3. WireMock.Net, 4.HttpClient, 5.NUnit.Framework.    

