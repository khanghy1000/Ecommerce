import { expect, describe, it, beforeAll, afterAll } from 'vitest';
import { Builder, By, until, WebDriver } from 'selenium-webdriver';
import chrome from 'selenium-webdriver/chrome';
import { CLIENT_URL } from './testConstants';

// Helper to get random unique elements from an array
function getRandomUnique<T>(arr: T[], count: number): T[] {
  const shuffled = arr.slice().sort(() => 0.5 - Math.random());
  return shuffled.slice(0, count);
}

describe('Product Search', () => {
  let driver: WebDriver;

  beforeAll(async () => {
    const options = new chrome.Options();
    // options.addArguments('--headless');
    driver = await new Builder()
      .forBrowser('chrome')
      .setChromeOptions(options)
      .build();
  });

  afterAll(async () => {
    await driver.quit();
  });

  it('should search for 3 random products from homepage', async () => {
    // Go to homepage
    await driver.get(`${CLIENT_URL}/`);

    // Wait for product carousels to load
    await driver.wait(
      until.elementsLocated(By.className('homepage-product-carousel')),
      10000
    );

    // Get all product cards on homepage (from all carousels)
    const productCardNameElements = await driver.findElements(
      By.className('product-card-name')
    );
    const productNames: string[] = [];
    for (const el of productCardNameElements) {
      const name = await el.getText();
      if (name && !productNames.includes(name)) productNames.push(name);
    }

    // Pick 3 random product names
    const randomNames = getRandomUnique(productNames, 3);
    expect(randomNames.length).toBe(3);

    for (const name of randomNames) {
      await driver.get(`${CLIENT_URL}/`);
      // Wait for search input
      const searchInput = await driver.wait(
        until.elementLocated(By.css('.search-input input')),
        5000
      );
      // Enter product name and search
      await searchInput.clear();
      await searchInput.sendKeys(name);
      await searchInput.sendKeys('\n');

      // Wait for product grid to load
      await driver.wait(
        until.elementLocated(By.className('product-search-grid')),
        10000
      );
      // Wait for at least one product card
      const cards = await driver.wait(
        until.elementsLocated(By.className('product-card')),
        10000
      );
      // Check at least one card is visible
      let found = false;
      for (const card of cards) {
        const cardNameEl = await card.findElement(
          By.className('product-card-name')
        );
        const cardName = await cardNameEl.getText();
        if (cardName.includes(name)) {
          found = true;
          break;
        }
      }
      expect(found).toBe(true);
    }
  });

  it('should show no results for non-existent product', async () => {
    // Go to homepage
    await driver.get(`${CLIENT_URL}/`);

    // Wait for product carousels to load
    await driver.wait(
      until.elementsLocated(By.className('homepage-product-carousel')),
      10000
    );

    // Search for a non-existent product
    const searchInput = await driver.wait(
      until.elementLocated(By.css('.search-input input')),
      5000
    );
    await searchInput.clear();
    await searchInput.sendKeys(
      'NonExistentProduct f768221f-309d-4b2b-968f-cf34901c2abc'
    );
    await searchInput.sendKeys('\n');

    // Wait for no results message
    const noResultsMessage = await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(), "No products found")]')
      ),
      10000
    );
    await driver.executeScript(
      'arguments[0].scrollIntoView(true);',
      noResultsMessage
    );
    // Check no results message is displayed
    expect(await noResultsMessage.isDisplayed()).toBe(true);
    await driver.sleep(2000);
  });
});
