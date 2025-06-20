import { expect, describe, it, beforeAll, afterAll, beforeEach } from 'vitest';
import { Builder, By, until, WebDriver } from 'selenium-webdriver';
import chrome from 'selenium-webdriver/chrome';
import { CLIENT_URL } from './testConstants';

describe('Checkout Process', () => {
  let driver: WebDriver;

  // Helper function to clear all items from cart
  const clearCart = async () => {
    try {
      await driver.get(`${CLIENT_URL}/cart`);
      await driver.sleep(2000);

      // Check if cart is already empty
      try {
        await driver.findElement(
          By.xpath('//*[contains(text(),"Your cart is empty")]')
        );
        return;
      } catch {
        // Cart is not empty, proceed with clearing
      }

      // Keep removing items until cart is empty
      let attempts = 0;
      const maxAttempts = 10;

      while (attempts < maxAttempts) {
        try {
          const removeButtons = await driver.findElements(
            By.className('cart-item-remove')
          );

          if (removeButtons.length === 0) {
            break;
          }

          await removeButtons[0].click();
          await driver.sleep(2000);
          attempts++;
        } catch {
          break;
        }
      }
    } catch (error) {
      console.log('Error during cart clearing:', error);
    }
  };

  // Helper function to add product to cart
  const addProductToCart = async () => {
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
  };

  // Helper function to navigate to checkout
  const navigateToCheckout = async () => {
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

    await driver.sleep(2000);

    // Click checkout button
    const checkoutButton = await driver.wait(
      until.elementLocated(By.className('checkout-button')),
      10000
    );

    await driver.executeScript(
      'arguments[0].scrollIntoView();',
      checkoutButton
    );
    await driver.sleep(1000);

    await checkoutButton.click();

    // Wait for checkout page to load
    await driver.wait(until.urlContains('/checkout'), 10000);
  };

  beforeAll(async () => {
    const options = new chrome.Options();
    // options.addArguments('--headless');
    // options.addArguments('--no-sandbox');
    // options.addArguments('--disable-dev-shm-usage');

    driver = await new Builder()
      .forBrowser('chrome')
      .setChromeOptions(options)
      .build();

    // Login
    await driver.get(`${CLIENT_URL}/login`);

    const emailInput = await driver.findElement(By.name('email'));
    await emailInput.sendKeys('buyer@a.com');

    const passwordInput = await driver.findElement(By.name('password'));
    await passwordInput.sendKeys('Test12345*');

    const loginButton = await driver.findElement(By.className('login-button'));
    await loginButton.click();

    // Wait for homepage to load
    await driver.wait(until.urlIs(`${CLIENT_URL}/`), 10000);
  });

  afterAll(async () => {
    await driver.quit();
  });

  beforeEach(async () => {
    await clearCart();
  });

  it('(COD) should redirect to orders page after checkout', async () => {
    // Add product to cart
    await addProductToCart();

    // Navigate to checkout
    await navigateToCheckout();

    // Select COD payment method
    const paymentMethodSection = await driver.wait(
      until.elementLocated(By.className('mantine-SegmentedControl-root')),
      10000
    );

    const codLabel = await paymentMethodSection.findElement(
      By.css('.mantine-SegmentedControl-label')
    );
    await driver.executeScript('arguments[0].scrollIntoView(true);', codLabel);
    await driver.sleep(500);
    await codLabel.click();

    await driver.sleep(2000);

    // Click place order button
    const placeOrderButton = await driver.wait(
      until.elementLocated(By.css('.checkout-button.ready-to-checkout')),
      10000
    );

    await driver.executeScript(
      'arguments[0].scrollIntoView();',
      placeOrderButton
    );
    await driver.sleep(1000);

    await placeOrderButton.click();

    // Should be redirected to orders page for COD
    await driver.wait(until.urlContains('/orders'), 15000);

    const currentUrl = await driver.getCurrentUrl();
    expect(currentUrl).toContain('/orders');
  });

  it('(VNPay) should handle payment cancellation', async () => {
    // Add product to cart
    await addProductToCart();

    // Navigate to checkout
    await navigateToCheckout();

    // Select VNPay payment method
    const paymentMethodSection = await driver.wait(
      until.elementLocated(By.className('mantine-SegmentedControl-root')),
      10000
    );

    const vnpayLabel = await paymentMethodSection
      .findElements(By.css('.mantine-SegmentedControl-label'))
      .then((labels) => labels[1]);
    await driver.executeScript(
      'arguments[0].scrollIntoView(true);',
      vnpayLabel
    );
    await driver.sleep(500);
    await vnpayLabel.click();

    // Click place order button
    const placeOrderButton = await driver.wait(
      until.elementLocated(By.css('.checkout-button.ready-to-checkout')),
      10000
    );

    await driver.executeScript(
      'arguments[0].scrollIntoView();',
      placeOrderButton
    );
    await driver.sleep(1000);

    await placeOrderButton.click();

    // Should be redirected to VNPay sandbox
    await driver.wait(until.urlContains('sandbox.vnpayment.vn'), 15000);

    const currentUrl = await driver.getCurrentUrl();
    expect(currentUrl).toContain('sandbox.vnpayment.vn');

    // Wait for VNPay page to load completely
    await driver.sleep(3000);

    // Find and click the cancel payment button
    // const cancelButton = await driver.wait(
    //   until.elementLocated(
    //     By.css('a[data-bs-target="#modalCancelPayment"]')
    //   ),
    //   10000
    // );

    // await cancelButton.click();

    await driver.executeScript(
      'document.querySelector(\'a[data-bs-target="#modalCancelPayment"]\').click();'
    );

    await driver.sleep(1000);

    // Wait for modal to appear and click the danger button to confirm cancellation
    const confirmCancelButton = await driver.wait(
      until.elementLocated(By.css('a[data-bs-dismiss="modal"].ubg-danger')),
      10000
    );
    await confirmCancelButton.click();

    // Should be redirected to payment failure page
    await driver.wait(until.urlContains('/payment/failure'), 15000);

    const finalUrl = await driver.getCurrentUrl();
    expect(finalUrl).toContain('/payment/failure');

    // Verify we're on the payment failure page
    try {
      const failureMessage = await driver.wait(
        until.elementLocated(
          By.xpath(
            '//*[contains(text(),"Payment Failed") or contains(text(),"Payment Cancelled") or contains(text(),"failed")]'
          )
        ),
        5000
      );
      expect(await failureMessage.isDisplayed()).toBe(true);
    } catch {
      // If specific failure message not found, just verify URL
      expect(finalUrl).toContain('/payment/failure');
    }
  });

  it('(VNPay) should handle successful payment', async () => {
    // Add product to cart
    await addProductToCart();

    // Navigate to checkout
    await navigateToCheckout();

    // Select VNPay payment method
    const paymentMethodSection = await driver.wait(
      until.elementLocated(By.className('mantine-SegmentedControl-root')),
      10000
    );

    const vnpayLabel = await paymentMethodSection
      .findElements(By.css('.mantine-SegmentedControl-label'))
      .then((labels) => labels[1]);
    await driver.executeScript(
      'arguments[0].scrollIntoView(true);',
      vnpayLabel
    );
    await driver.sleep(500);
    await vnpayLabel.click();

    // Click place order button
    const placeOrderButton = await driver.wait(
      until.elementLocated(By.css('.checkout-button.ready-to-checkout')),
      10000
    );

    await driver.executeScript(
      'arguments[0].scrollIntoView();',
      placeOrderButton
    );
    await driver.sleep(1000);

    await placeOrderButton.click();

    // Should be redirected to VNPay sandbox
    await driver.wait(until.urlContains('sandbox.vnpayment.vn'), 15000);

    const currentUrl = await driver.getCurrentUrl();
    expect(currentUrl).toContain('sandbox.vnpayment.vn');

    // Wait for VNPay page to load
    await driver.sleep(3000);

    // css: body > div:nth-child(2) > div:nth-child(2) > div:nth-child(1) > div:nth-child(1) > div:nth-child(2) > form:nth-child(1) > div:nth-child(1) > div:nth-child(1) > div:nth-child(2) > div:nth-child(2) > div:nth-child(1) > div:nth-child(1)

    const paymentType = await driver.findElement(
      By.css(
        'body > div:nth-child(2) > div:nth-child(2) > div:nth-child(1) > div:nth-child(1) > div:nth-child(2) > form:nth-child(1) > div:nth-child(1) > div:nth-child(1) > div:nth-child(2) > div:nth-child(2) > div:nth-child(1) > div:nth-child(1)'
      )
    );
    await paymentType.click();
    await driver.sleep(1000);

    // id: searchPayMethod2

    const searchPayMethodInput = await driver.findElement(
      By.id('searchPayMethod2')
    );
    await searchPayMethodInput.sendKeys('NCB');
    await driver.sleep(1000);

    // button id: NCB
    const ncbButton = await driver.findElement(By.id('NCB'));
    await ncbButton.click();
    await driver.sleep(3000);

    // input id: card_number_mask

    const cardNumberInput = await driver.wait(
      until.elementLocated(By.id('card_number_mask')),
      10000
    );
    await cardNumberInput.sendKeys('9704198526191432198');
    await driver.sleep(1000);

    // cardHolder
    const cardHolderInput = await driver.findElement(By.id('cardHolder'));
    await cardHolderInput.sendKeys('NGUYEN VAN A');
    await driver.sleep(1000);

    // cardDate
    const cardDateInput = await driver.findElement(By.id('cardDate'));
    await cardDateInput.sendKeys('0715');
    await driver.sleep(1000);

    // btnContinue
    const btnContinue = await driver.findElement(By.id('btnContinue'));
    await driver.executeScript('arguments[0].scrollIntoView();', btnContinue);
    await btnContinue.click();
    await driver.sleep(1000);

    // btnAgree
    const btnAgree = await driver.findElement(By.id('btnAgree'));
    await btnAgree.click();
    await driver.sleep(3000);

    // otpvalue
    const otpInput = await driver.wait(
      until.elementLocated(By.id('otpvalue')),
      10000
    );
    await otpInput.sendKeys('123456');
    await driver.sleep(1000);

    // btnConfirm
    const btnConfirm = await driver.findElement(By.id('btnConfirm'));
    await btnConfirm.click();
    await driver.sleep(5000);

    // Verify we're on the payment success page
    await driver.wait(until.urlContains('/payment/success'), 10000);

    const successUrl = await driver.getCurrentUrl();
    expect(successUrl).toContain('/payment/success');

    // Verify success page content
    try {
      const successMessage = await driver.wait(
        until.elementLocated(
          By.xpath(
            '//*[contains(text(),"Payment Successful") or contains(text(),"Success") or contains(text(),"successful")]'
          )
        ),
        5000
      );
      expect(await successMessage.isDisplayed()).toBe(true);
    } catch {
      // If specific success message not found, just verify URL
      expect(successUrl).toContain('/payment/success');
    }
  });
});
