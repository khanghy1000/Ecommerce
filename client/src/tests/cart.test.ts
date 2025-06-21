import { expect, describe, it, beforeAll, afterAll, afterEach } from 'vitest';
import { Builder, By, until, WebDriver, Key } from 'selenium-webdriver';
import chrome from 'selenium-webdriver/chrome';
import { CLIENT_URL } from './testConstants';

describe('Cart Management', () => {
  let driver: WebDriver;

  // Helper function to clear all items from cart
  const clearCart = async () => {
    try {
      // Navigate to cart page
      await driver.get(`${CLIENT_URL}/cart`);

      // Wait for page to load
      await driver.sleep(2000);

      // Look for empty cart message first
      try {
        await driver.findElement(
          By.xpath('//*[contains(text(),"Your cart is empty")]')
        );
        console.log('Cart is already empty');
        return;
      } catch {
        // Cart is not empty, proceed with clearing
      }

      // Keep removing items until cart is empty (max 10 iterations)
      let attempts = 0;
      const maxAttempts = 10;

      while (attempts < maxAttempts) {
        try {
          // Look for any remove button
          const removeButtons = await driver.findElements(
            By.className('cart-item-remove')
          );

          if (removeButtons.length === 0) {
            console.log('No more items to remove');
            break;
          }

          // Click the first remove button
          await removeButtons[0].click();

          // Wait for the removal to complete
          await driver.sleep(2000);

          attempts++;
        } catch (error) {
          console.log('Error removing item:', error);
          break;
        }
      }

      console.log(`Cart cleared after ${attempts} removals`);
    } catch (error) {
      console.log('Error during cart clearing:', error);
    }
  };

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

    await driver.get(`${CLIENT_URL}/login`);

    const emailInput = await driver.findElement(By.name('email'));
    await emailInput.sendKeys('buyer@a.com');

    const passwordInput = await driver.findElement(By.name('password'));
    await passwordInput.sendKeys('Test12345*');

    const loginButton = await driver.findElement(By.className('login-button'));
    await loginButton.click();

    // Wait for homepage to load
    await driver.wait(until.urlIs(`${CLIENT_URL}/`), 10000);

    // Clear cart before each test
    await clearCart();
  });

  afterAll(async () => {
    await driver.quit();
  });

  afterEach(async () => {
    await clearCart();
  });

  it('should add product to cart', async () => {
    await driver.get(`${CLIENT_URL}/`);

    // Wait for homepage to load and products to appear
    await driver.wait(
      until.elementLocated(By.className('product-card')),
      10000
    );

    // Find the first product card and click it
    const productCard = await driver.findElement(By.className('product-card'));
    await productCard.click();

    // Wait for product page to load
    await driver.wait(
      until.elementLocated(By.className('add-to-cart-button')),
      10000
    );

    const quantityInput = await driver.findElement(
      By.css('.product-quantity-input input')
    );

    const increaseButton = await driver.findElement(
      By.css(
        '.product-quantity-input button.mantine-NumberInput-control[data-direction="up"]'
      )
    );
    await increaseButton.click();

    const quantity = await quantityInput.getAttribute('value');

    // Click Add to Cart button
    const addToCartButton = await driver.findElement(
      By.className('add-to-cart-button')
    );
    await addToCartButton.click();

    // Wait for success notification
    await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(),"Product added to cart")]')
      ),
      10000
    );

    // Navigate to cart
    const cartNavButton = await driver.findElement(
      By.className('cart-nav-button')
    );
    await cartNavButton.click();

    // Wait for cart page to load
    await driver.wait(until.urlIs(`${CLIENT_URL}/cart`), 10000);

    // Verify product is in cart with correct quantity
    const cartQuantityInput = await driver.wait(
      until.elementLocated(By.css('.cart-item-quantity input')),
      10000
    );
    const quantityValue = await cartQuantityInput.getAttribute('value');
    expect(quantityValue).toBe(quantity);
  });

  it('should update cart item quantity', async () => {
    // First add a product to cart
    await driver.get(`${CLIENT_URL}/`);
    await driver.wait(
      until.elementLocated(By.className('product-card')),
      10000
    );

    const productCard = await driver.findElement(By.className('product-card'));
    await productCard.click();

    await driver.wait(
      until.elementLocated(By.className('add-to-cart-button')),
      10000
    );

    const addToCartButton = await driver.findElement(
      By.className('add-to-cart-button')
    );
    await addToCartButton.click();

    // Wait for success notification
    await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(),"Product added to cart")]')
      ),
      10000
    );

    // Navigate to cart
    const cartNavButton = await driver.findElement(
      By.className('cart-nav-button')
    );
    await cartNavButton.click();

    await driver.wait(until.urlIs(`${CLIENT_URL}/cart`), 10000);

    // Update quantity to 5
    const cartQuantityInput = await driver.wait(
      until.elementLocated(By.css('.cart-item-quantity input')),
      10000
    );
    await cartQuantityInput.clear();
    await cartQuantityInput.sendKeys('5');
    await cartQuantityInput.sendKeys(Key.TAB); // Trigger onChange

    await driver.sleep(1000);

    // Verify quantity was updated
    const updatedQuantityValue = await cartQuantityInput.getAttribute('value');
    expect(updatedQuantityValue).toBe('5');
  });

  it('should handle maximum quantity limit', async () => {
    // First go to homepage and select a product
    await driver.get(`${CLIENT_URL}/`);
    await driver.wait(
      until.elementLocated(By.className('product-card')),
      10000
    );

    const productCard = await driver.findElement(By.className('product-card'));
    await productCard.click();

    // Wait for product page to load
    await driver.wait(
      until.elementLocated(By.className('add-to-cart-button')),
      10000
    );

    // Get the product's stock quantity from the availability text
    const availabilityText = await driver.findElement(
      By.className('product-quantity')
    );
    const availabilityTextContent = await availabilityText.getText();
    const stockQuantity = parseInt(availabilityTextContent.split(' ')[0]);

    // Add product to cart
    const addToCartButton = await driver.findElement(
      By.className('add-to-cart-button')
    );
    await addToCartButton.click();

    // Wait for success notification
    await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(),"Product added to cart")]')
      ),
      10000
    );

    // Navigate to cart
    const cartNavButton = await driver.findElement(
      By.className('cart-nav-button')
    );
    await cartNavButton.click();

    await driver.wait(until.urlIs(`${CLIENT_URL}/cart`), 10000);

    // Try to set quantity higher than stock
    const cartQuantityInput = await driver.wait(
      until.elementLocated(By.css('.cart-item-quantity input')),
      10000
    );

    const excessiveQuantity = stockQuantity + 5;
    await cartQuantityInput.clear();
    await cartQuantityInput.sendKeys(excessiveQuantity.toString());
    await cartQuantityInput.sendKeys(Key.TAB); // Trigger onChange

    // Check for max quantity warning text
    const maxQuantityWarning = await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(),"Not enough product in stock")]')
      ),
      10000
    );
    expect(await maxQuantityWarning.isDisplayed()).toBe(true);
  });

  it('should remove item from cart', async () => {
    // First add a product to cart
    await driver.get(`${CLIENT_URL}/`);
    await driver.wait(
      until.elementLocated(By.className('product-card')),
      10000
    );

    const productCard = await driver.findElement(By.className('product-card'));
    await productCard.click();

    await driver.wait(
      until.elementLocated(By.className('add-to-cart-button')),
      10000
    );

    const addToCartButton = await driver.findElement(
      By.className('add-to-cart-button')
    );
    await addToCartButton.click();

    // Wait for success notification
    await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(),"Product added to cart")]')
      ),
      10000
    );

    // Navigate to cart
    const cartNavButton = await driver.findElement(
      By.className('cart-nav-button')
    );
    await cartNavButton.click();

    await driver.wait(until.urlIs(`${CLIENT_URL}/cart`), 10000);

    // Click remove button
    const removeButton = await driver.wait(
      until.elementLocated(By.className('cart-item-remove')),
      10000
    );
    await removeButton.click();

    // Wait for item to be removed - cart should show empty state
    await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(),"Your cart is empty")]')
      ),
      10000
    );

    // Verify empty cart message is displayed
    const emptyMessage = await driver.findElement(
      By.xpath('//*[contains(text(),"Your cart is empty")]')
    );
    expect(await emptyMessage.isDisplayed()).toBe(true);
  });

  it('should proceed to checkout with selected items', async () => {
    // Add a product to cart
    await driver.get(`${CLIENT_URL}/`);
    await driver.wait(
      until.elementLocated(By.className('product-card')),
      10000
    );

    const productCard = await driver.findElement(By.className('product-card'));
    await productCard.click();

    await driver.wait(
      until.elementLocated(By.className('add-to-cart-button')),
      10000
    );

    const addToCartButton = await driver.findElement(
      By.className('add-to-cart-button')
    );
    await addToCartButton.click();

    await driver.wait(
      until.elementLocated(
        By.xpath('//*[contains(text(),"Product added to cart")]')
      ),
      10000
    );

    // Navigate to cart
    const cartNavButton = await driver.findElement(
      By.className('cart-nav-button')
    );
    await cartNavButton.click();

    await driver.wait(until.urlIs(`${CLIENT_URL}/cart`), 10000);

    // Select the item
    const checkbox = await driver.wait(
      until.elementLocated(By.className('cart-item-checkbox')),
      10000
    );

    const isChecked = await checkbox.isSelected();
    if (!isChecked) {
      await checkbox.click();
    }

    const cartItemName = await driver.findElement(
      By.className('cart-item-name')
    );
    const cartItemNameText = await cartItemName.getText();

    // Click checkout button - scroll to it first to ensure it's clickable
    const checkoutButton = await driver.wait(
      until.elementLocated(By.className('checkout-button')),
      10000
    );

    // Scroll to the button to ensure it's in view and clickable
    await driver.executeScript(
      'arguments[0].scrollIntoView();',
      checkoutButton
    );
    await driver.sleep(1000);

    await checkoutButton.click();

    // Should be redirected to checkout page
    await driver.wait(until.urlContains('/checkout'), 10000);

    const currentUrl = await driver.getCurrentUrl();

    const itemNameInCheckout = await driver.wait(
      until.elementLocated(By.className('product-name')),
      10000
    );
    const itemNameText = await itemNameInCheckout.getText();

    expect(currentUrl).toContain('/checkout');
    expect(itemNameText).toContain(cartItemNameText);
  });
});
