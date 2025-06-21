import { Builder, By, until, WebDriver } from 'selenium-webdriver';
import * as chrome from 'selenium-webdriver/chrome';
import { afterAll, beforeAll, describe, expect, it } from 'vitest';

const CLIENT_URL = 'http://localhost:5173';

describe('Product Management - Add Product', () => {
  let driver: WebDriver;

  beforeAll(async () => {
    const options = new chrome.Options();
    options.addArguments('--start-maximized');
    // options.addArguments('--headless');
    // options.addArguments('--no-sandbox');
    // options.addArguments('--disable-dev-shm-usage');

    driver = await new Builder()
      .forBrowser('chrome')
      .setChromeOptions(options)
      .build();

    // Login as shop user first
    await driver.get(`${CLIENT_URL}/login`);

    const emailInput = await driver.findElement(By.name('email'));
    await emailInput.sendKeys('shop@a.com');

    const passwordInput = await driver.findElement(By.name('password'));
    await passwordInput.sendKeys('Test12345*');

    const loginButton = await driver.findElement(By.className('login-button'));
    await loginButton.click();

    // Wait for successful login and redirect
    await driver.wait(until.urlIs(`${CLIENT_URL}/`), 10000);
  });

  afterAll(async () => {
    await driver.quit();
  });

  it('should show validation errors when creating product with no input', async () => {
    // Navigate to product create page before each test
    await driver.get(`${CLIENT_URL}/management`);
    const productManagementLink = await driver.wait(
      until.elementLocated(By.className('products-link')),
      10000
    );
    await productManagementLink.click();

    const createProductButton = await driver.wait(
      until.elementLocated(By.className('create-product-button')),
      10000
    );
    await createProductButton.click();

    // Wait for the form to load
    await driver.wait(
      until.elementLocated(By.className('product-form')),
      10000
    );

    // Click create product button without filling any fields
    const createButton = await driver.findElement(
      By.className('product-form-submit-button')
    );
    await createButton.click();

    // Wait for and verify validation errors
    const nameError = await driver.wait(
      until.elementLocated(
        By.xpath(
          '//*[contains(text(),"Product name must be at least 3 characters")]'
        )
      ),
      5000
    );
    expect(await nameError.isDisplayed()).toBe(true);

    const descriptionError = await driver.wait(
      until.elementLocated(
        By.xpath(
          '//*[contains(text(),"Description must be at least 10 characters")]'
        )
      ),
      5000
    );

    expect(await descriptionError.isDisplayed()).toBe(true);

    const priceError = await driver.wait(
      until.elementLocated(
        By.xpath(
          '//*[contains(text(),"Regular price must be at least 1000 VND")]'
        )
      ),
      5000
    );
    expect(await priceError.isDisplayed()).toBe(true);

    // Verify form doesn't submit (still on create page)
    const currentUrl = await driver.getCurrentUrl();
    expect(currentUrl).toBe(`${CLIENT_URL}/management/products/create`);
    await driver.sleep(2000);
  });

  it('should show validation error for product name with only 2 characters', async () => {
    // Navigate to product create page before each test
    await driver.get(`${CLIENT_URL}/management`);
    const productManagementLink = await driver.wait(
      until.elementLocated(By.className('products-link')),
      10000
    );
    await productManagementLink.click();

    const createProductButton = await driver.wait(
      until.elementLocated(By.className('create-product-button')),
      10000
    );
    await createProductButton.click();

    // Wait for the form to load
    await driver.wait(
      until.elementLocated(By.className('product-form')),
      10000
    );

    // Fill valid description and price
    const descriptionEditor = await driver.findElement(
      By.css('.product-form-description .ProseMirror')
    );
    await descriptionEditor.click();
    await descriptionEditor.sendKeys(
      'This is a valid product description with more than 10 characters.'
    );

    const priceInput = await driver.findElement(
      By.css('.product-form-price input')
    );
    await priceInput.clear();
    await priceInput.sendKeys('15000');

    // Fill quantity, dimensions, weight
    const quantityInput = await driver.findElement(
      By.css('.product-form-quantity input')
    );
    await quantityInput.clear();
    await quantityInput.sendKeys('10');

    const lengthInput = await driver.findElement(
      By.css('.product-form-length input')
    );
    await lengthInput.clear();
    await lengthInput.sendKeys('20');

    const widthInput = await driver.findElement(
      By.css('.product-form-width input')
    );
    await widthInput.clear();
    await widthInput.sendKeys('15');

    const heightInput = await driver.findElement(
      By.css('.product-form-height input')
    );
    await heightInput.clear();
    await heightInput.sendKeys('10');

    const weightInput = await driver.findElement(
      By.css('.product-form-weight input')
    );
    await weightInput.clear();
    await weightInput.sendKeys('500');

    // Fill product name with only 2 characters
    const nameInput = await driver.findElement(
      By.css('.product-form-name input')
    );
    await nameInput.clear();
    await nameInput.sendKeys('AB');
    await driver.sleep(2000);

    // Submit form
    const createButton = await driver.findElement(
      By.className('product-form-submit-button')
    );
    await createButton.click();

    // Wait for and verify name validation error
    const nameError = await driver.wait(
      until.elementLocated(
        By.xpath(
          '//*[contains(text(),"Product name must be at least 3 characters")]'
        )
      ),
      5000
    );
    expect(await nameError.isDisplayed()).toBe(true);

    // Verify form doesn't submit (still on create page)
    const currentUrl = await driver.getCurrentUrl();
    expect(currentUrl).toBe(`${CLIENT_URL}/management/products/create`);
    await driver.sleep(2000);
  });

  it('should successfully create product with all valid input', async () => {
    // Navigate to product create page before each test
    await driver.get(`${CLIENT_URL}/management`);
    const productManagementLink = await driver.wait(
      until.elementLocated(By.className('products-link')),
      10000
    );
    await productManagementLink.click();

    const createProductButton = await driver.wait(
      until.elementLocated(By.className('create-product-button')),
      10000
    );
    await createProductButton.click();

    // Wait for the form to load
    await driver.wait(
      until.elementLocated(By.className('product-form')),
      10000
    );

    // Fill product name
    const nameInput = await driver.findElement(
      By.css('.product-form-name input')
    );
    await nameInput.clear();
    const productName = `Test Product ${Date.now()}`;
    await nameInput.sendKeys(productName);

    // Fill description using rich text editor
    const descriptionEditor = await driver.findElement(
      By.css('.product-form-description .ProseMirror')
    );
    await descriptionEditor.click();
    await descriptionEditor.sendKeys(
      'This is a comprehensive test product description with detailed information about the product features and benefits.'
    );

    // Fill regular price
    const priceInput = await driver.findElement(
      By.css('.product-form-price input')
    );
    await priceInput.clear();
    await priceInput.sendKeys('25000');

    // Fill quantity
    const quantityInput = await driver.findElement(
      By.css('.product-form-quantity input')
    );
    await quantityInput.clear();
    await quantityInput.sendKeys('50');

    // Fill dimensions
    const lengthInput = await driver.findElement(
      By.css('.product-form-length input')
    );
    await lengthInput.clear();
    await lengthInput.sendKeys('30');

    const widthInput = await driver.findElement(
      By.css('.product-form-width input')
    );
    await widthInput.clear();
    await widthInput.sendKeys('20');

    const heightInput = await driver.findElement(
      By.css('.product-form-height input')
    );
    await heightInput.clear();
    await heightInput.sendKeys('15');

    // Fill weight
    const weightInput = await driver.findElement(
      By.css('.product-form-weight input')
    );
    await weightInput.clear();
    await weightInput.sendKeys('750');

    // Wait a moment for form to process all inputs
    await driver.sleep(1000);

    // Submit form
    const createButton = await driver.findElement(
      By.className('product-form-submit-button')
    );
    await createButton.click();

    // Wait for success notification or redirect to edit page
    // Check for success notification
    await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(), "Product created")]')
      ),
      10000
    );

    // Verify we're no longer on the create page
    const currentUrl = await driver.getCurrentUrl();
    expect(currentUrl).not.toBe(`${CLIENT_URL}/management/products/create`);

    // If redirected to edit page, verify the product name is there
    if (currentUrl.includes('/edit/')) {
      const editNameInput = await driver.wait(
        until.elementLocated(By.css('.product-form-name input')),
        5000
      );
      const displayedName = await editNameInput.getAttribute('value');
      expect(displayedName).toBe(productName);
      await driver.sleep(2000);
    }
  });

  it('should validate minimum price requirement', async () => {
    // Navigate to product create page before each test
    await driver.get(`${CLIENT_URL}/management`);
    const productManagementLink = await driver.wait(
      until.elementLocated(By.className('products-link')),
      10000
    );
    await productManagementLink.click();

    const createProductButton = await driver.wait(
      until.elementLocated(By.className('create-product-button')),
      10000
    );
    await createProductButton.click();

    // Wait for the form to load
    await driver.wait(
      until.elementLocated(By.className('product-form')),
      10000
    );

    // Fill valid name and description
    const nameInput = await driver.findElement(
      By.css('.product-form-name input')
    );
    await nameInput.clear();
    await nameInput.sendKeys('Valid Product Name');

    const descriptionEditor = await driver.findElement(
      By.css('.product-form-description .ProseMirror')
    );
    await descriptionEditor.click();
    await descriptionEditor.sendKeys(
      'This is a valid product description with more than 10 characters.'
    );

    // Fill other required fields
    const quantityInput = await driver.findElement(
      By.css('.product-form-quantity input')
    );
    await quantityInput.clear();
    await quantityInput.sendKeys('10');

    const lengthInput = await driver.findElement(
      By.css('.product-form-length input')
    );
    await lengthInput.clear();
    await lengthInput.sendKeys('20');

    const widthInput = await driver.findElement(
      By.css('.product-form-width input')
    );
    await widthInput.clear();
    await widthInput.sendKeys('15');

    const heightInput = await driver.findElement(
      By.css('.product-form-height input')
    );
    await heightInput.clear();
    await heightInput.sendKeys('10');

    const weightInput = await driver.findElement(
      By.css('.product-form-weight input')
    );
    await weightInput.clear();
    await weightInput.sendKeys('500');

    // Submit form
    const createButton = await driver.findElement(
      By.className('product-form-submit-button')
    );
    await createButton.click();

    // Wait for and verify price validation error
    const priceError = await driver.wait(
      until.elementLocated(
        By.xpath(
          '//*[contains(text(),"Regular price must be at least 1000 VND")]'
        )
      ),
      5000
    );
    expect(await priceError.isDisplayed()).toBe(true);

    // Verify form doesn't submit (still on create page)
    const currentUrl = await driver.getCurrentUrl();
    expect(currentUrl).toBe(`${CLIENT_URL}/management/products/create`);
    await driver.sleep(2000);
  });
});
