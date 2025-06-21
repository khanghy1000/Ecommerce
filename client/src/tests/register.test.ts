import { expect, describe, it, beforeAll, afterAll } from 'vitest';
import { Builder, By, until, WebDriver, Key } from 'selenium-webdriver';
import chrome from 'selenium-webdriver/chrome';
import { CLIENT_URL } from './testConstants';

// Helper to generate a unique email for registration
function generateUniqueEmail() {
  return `testuser_${Date.now()}@example.com`;
}

describe('Register', () => {
  let driver: WebDriver;

  beforeAll(async () => {
    const options = new chrome.Options();
    options.addArguments('--start-maximized');
    // options.addArguments('--headless');
    driver = await new Builder()
      .forBrowser('chrome')
      .setChromeOptions(options)
      .build();
  });

  afterAll(async () => {
    await driver.quit();
  });

  it('should show error when missing input', async () => {
    await driver.get(`${CLIENT_URL}/register`);
    // Wait for the page to load
    await driver.wait(
      until.elementLocated(By.className('register-button')),
      5000
    );
    // Click register without filling anything
    const registerButton = await driver.findElement(
      By.className('register-button')
    );
    await registerButton.click();
    // Wait for error messages to appear (Mantine shows errors under inputs)
    const errorInputs = [
      'register-displayName',
      'register-email',
      'register-password',
      'register-phoneNumber',
      'register-address',
    ];
    for (const className of errorInputs) {
      const input = await driver.findElement(By.className(className));
      // Error message is shown as sibling with class mantine-InputWrapper-error
      const error = await input.findElement(
        By.className('mantine-InputWrapper-error')
      );
      expect(await error.isDisplayed()).toBe(true);
    }
  });

  it('should show error when wrong email format', async () => {
    await driver.get(`${CLIENT_URL}/register`);
    // Wait for the page to load
    await driver.wait(
      until.elementLocated(By.className('register-button')),
      5000
    );

    await driver
      .findElement(By.css('.register-email input'))
      .sendKeys('wrongemailformat');

    const registerButton = await driver.findElement(
      By.className('register-button')
    );
    await registerButton.click();
    // Wait for error messages to appear (Mantine shows errors under inputs)
    const input = await driver.findElement(By.className('register-email'));
    // Error message is shown as sibling with class mantine-InputWrapper-error
    const error = await input.findElement(
      By.className('mantine-InputWrapper-error')
    );
    expect(await error.isDisplayed()).toBe(true);
    await driver.sleep(1000);
  });

  it('should show error when wrong password format', async () => {
    await driver.get(`${CLIENT_URL}/register`);
    // Wait for the page to load
    await driver.wait(
      until.elementLocated(By.className('register-button')),
      5000
    );

    await driver
      .findElement(By.css('.register-password input'))
      .sendKeys('wrongpasswordformat'); // No uppercase, no special char, no number

    const registerButton = await driver.findElement(
      By.className('register-button')
    );
    await registerButton.click();
    // Wait for error messages to appear (Mantine shows errors under inputs)
    const input = await driver.findElement(By.className('register-password'));
    // Error message is shown as sibling with class mantine-InputWrapper-error
    const error = await input.findElement(
      By.className('mantine-InputWrapper-error')
    );
    expect(await error.isDisplayed()).toBe(true);
    await driver.sleep(1000);
  });

  it('should show error when wrong phone number format', async () => {
    await driver.get(`${CLIENT_URL}/register`);
    // Wait for the page to load
    await driver.wait(
      until.elementLocated(By.className('register-button')),
      5000
    );

    await driver
      .findElement(By.css('.register-phoneNumber input'))
      .sendKeys('01234'); // Too short, should be 10 digits

    const registerButton = await driver.findElement(
      By.className('register-button')
    );

    await registerButton.click();
    // Wait for error messages to appear (Mantine shows errors under inputs)
    const input = await driver.findElement(
      By.className('register-phoneNumber')
    );
    // Error message is shown as sibling with class mantine-InputWrapper-error
    const error = await input.findElement(
      By.className('mantine-InputWrapper-error')
    );
    expect(await error.isDisplayed()).toBe(true);
    await driver.sleep(1000);
  });

  it('should register successfully with valid input', async () => {
    await driver.get(`${CLIENT_URL}/register`);
    await driver.wait(
      until.elementLocated(By.className('register-button')),
      5000
    );
    // Fill in the form
    await driver
      .findElement(By.css('.register-displayName input'))
      .sendKeys('Test User');
    const email = generateUniqueEmail();
    await driver.findElement(By.css('.register-email input')).sendKeys(email);
    await driver
      .findElement(By.css('.register-password input'))
      .sendKeys('Test12345*');
    await driver
      .findElement(By.css('.register-phoneNumber input'))
      .sendKeys('0123456789');
    await driver
      .findElement(By.css('.register-address input'))
      .sendKeys('123 Test Street');
    // Province
    const provinceSelect = await driver.findElement(
      By.css('.register-province input')
    );
    await provinceSelect.click();
    await driver.sleep(500); // Wait for dropdown
    // Select first option
    await provinceSelect.sendKeys(Key.ARROW_DOWN, Key.ENTER);
    // District
    const districtSelect = await driver.findElement(
      By.css('.register-district input')
    );
    await districtSelect.click();
    await driver.sleep(500);
    await districtSelect.sendKeys(Key.ARROW_DOWN, Key.ENTER);
    // Ward
    const wardSelect = await driver.findElement(By.css('.register-ward input'));
    await wardSelect.click();
    await driver.sleep(500);
    await wardSelect.sendKeys(Key.ARROW_DOWN, Key.ENTER);
    // Role (optional, default is Buyer)
    // Submit
    const registerButton = await driver.findElement(
      By.className('register-button')
    );
    await registerButton.click();
    // Wait for navigation to login page (after success)
    await driver.wait(until.urlContains('/login'), 10000);
    // Check for success notification (Mantine notification)
    const notification = await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(),"Registration successful")]')
      ),
      5000
    );
    await driver.wait(until.elementIsVisible(notification), 2000);
    // Verify notification is displayed
    expect(await notification.isDisplayed()).toBe(true);
    await driver.sleep(2000);
  });
});
