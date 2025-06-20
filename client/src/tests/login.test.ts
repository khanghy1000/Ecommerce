import { expect, describe, it, beforeAll, afterAll } from 'vitest';
import { Builder, By, until, WebDriver } from 'selenium-webdriver';
import chrome from 'selenium-webdriver/chrome';
import { CLIENT_URL } from './testConstants';

describe('Login', () => {
  let driver: WebDriver;

  beforeAll(async () => {
    // Set up Chrome options
    const options = new chrome.Options();
    // options.addArguments('--headless'); // Run in headless mode
    // options.addArguments('--no-sandbox');
    // options.addArguments('--disable-dev-shm-usage');

    // Build the driver
    driver = await new Builder()
      .forBrowser('chrome')
      .setChromeOptions(options)
      .build();
  });

  afterAll(async () => {
    // Close the browser after tests
    await driver.quit();
  });

  it('should login successfully with correct credentials', async () => {
    // Navigate to the login page
    await driver.get(`${CLIENT_URL}/login`);

    // Wait for the page to load and verify we are on the login page
    const loginTitleElement = await driver.wait(
      until.elementLocated(By.className('login-title')),
      5000
    );
    expect(await loginTitleElement.isDisplayed()).toBe(true);

    // Fill in the login form
    const emailInput = await driver.findElement(By.name('email'));
    await emailInput.sendKeys('buyer@a.com');

    const passwordInput = await driver.findElement(By.name('password'));
    await passwordInput.sendKeys('Test12345*');

    // Submit the form
    const loginButton = await driver.findElement(By.className('login-button'));
    await loginButton.click();

    // Wait for successful navigation to homepage after login
    await driver.wait(until.urlIs(`${CLIENT_URL}/`), 10000);

    // Verify we are on the homepage
    const currentUrl = await driver.getCurrentUrl();
    expect(currentUrl).toBe(`${CLIENT_URL}/`);

    // Verify user is logged in by checking for user avatar/menu button
    // The user menu is a button with an avatar and display name
    const userProfileElement = await driver.wait(
      until.elementLocated(
        By.xpath(
          '//button[contains(@class, "mantine-Button-root") and .//div[contains(@class, "mantine-Avatar-root")]]'
        )
      ),
      10000
    );
    expect(await userProfileElement.isDisplayed()).toBe(true);

    // Logout
    await userProfileElement.click();
    await driver.sleep(300);
    const logoutButton = await driver.wait(
      until.elementLocated(By.className('logout-button')),
      5000
    );
    await driver.wait(until.elementIsVisible(logoutButton), 2000);
    await logoutButton.click();
  });

  it('should show error message with incorrect credentials', async () => {
    // Navigate to the login page
    await driver.get(`${CLIENT_URL}/login`);

    // Wait for the page to load
    await driver.wait(until.elementLocated(By.className('login-title')), 5000);

    // Fill in the login form with incorrect credentials
    const emailInput = await driver.findElement(By.name('email'));
    await emailInput.sendKeys('buyer@a.com');

    const passwordInput = await driver.findElement(By.name('password'));
    await passwordInput.sendKeys('WrongPassword123');

    // Submit the form
    const loginButton = await driver.findElement(By.className('login-button'));
    await loginButton.click();

    // Wait for error message to appear - Mantine form errors appear as error text under the input
    const errorMessage = await driver.wait(
      until.elementLocated(
        By.className('mantine-InputWrapper-error mantine-PasswordInput-error')
      ),
      10000
    );

    // Verify error message is displayed
    expect(await errorMessage.isDisplayed()).toBe(true);
    await driver.sleep(1000);
  });
});
