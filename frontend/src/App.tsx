import { useEffect, useState } from "react";
import "./App.css";

type MenuItem = {
    id: number;
    name: string;
    description: string;
    price: number;
    imageUrl: string;
};

type CartItem = MenuItem & {
  quantity: number;
};

function App() {
    const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");
    const [cartItems, setCartItems] = useState<CartItem[]>([]);
    const [customerName, setCustomerName] = useState("");
const [deliveryAddress, setDeliveryAddress] = useState("");
const [phoneNumber, setPhoneNumber] = useState("");
const [orderMessage, setOrderMessage] = useState("");
const [isSubmitting, setIsSubmitting] = useState(false);
const [latestOrderId, setLatestOrderId] = useState<number | null>(null);
const [latestOrderStatus, setLatestOrderStatus] = useState("");

    useEffect(() => {
        async function loadMenu() {
            try {
                const response = await fetch("http://localhost:5000/api/menu");

                if (!response.ok) {
                    throw new Error("Could not load the menu.");
                }

                const data: MenuItem[] = await response.json();
                setMenuItems(data);
            } catch {
                setError("Could not load menu. Is the backend running?");
            } finally {
                setIsLoading(false);
            }
        }

        loadMenu();
    }, []);

    useEffect(() => {
  if (latestOrderId === null) {
    return;
  }

  async function refreshOrderStatus() {
    try {
      const response = await fetch(
        `http://localhost:5000/api/orders/${latestOrderId}`
      );

      if (!response.ok) {
        return;
      }

      const order = await response.json();
      setLatestOrderStatus(order.status);
    } catch {
      // Keep showing the last known status if refresh fails.
    }
  }

  refreshOrderStatus();

  const intervalId = window.setInterval(refreshOrderStatus, 5000);

  return () => window.clearInterval(intervalId);
}, [latestOrderId]);

function addToCart(menuItem: MenuItem) {
  setCartItems((currentItems) => {
    const existingItem = currentItems.find(
      (item) => item.id === menuItem.id
    );

    if (existingItem) {
      return currentItems.map((item) =>
        item.id === menuItem.id
          ? { ...item, quantity: item.quantity + 1 }
          : item
      );
    }

    return [...currentItems, { ...menuItem, quantity: 1 }];
  });
}

function changeQuantity(menuItemId: number, change: number) {
  setCartItems((currentItems) =>
    currentItems
      .map((item) =>
        item.id === menuItemId
          ? { ...item, quantity: item.quantity + change }
          : item
      )
      .filter((item) => item.quantity > 0)
  );
}

async function placeOrder(event: React.FormEvent<HTMLFormElement>) {
  event.preventDefault();

  if (cartItems.length === 0) {
    setOrderMessage("Add at least one item to your cart.");
    return;
  }

  setIsSubmitting(true);
  setOrderMessage("");

  try {
    const response = await fetch("http://localhost:5000/api/orders", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        customerName,
        deliveryAddress,
        phoneNumber,
        items: cartItems.map((item) => ({
          menuItemId: item.id,
          quantity: item.quantity,
        })),
      }),
    });

    if (!response.ok) {
      throw new Error("Could not place the order.");
    }

    const order = await response.json();
    setLatestOrderId(order.id);
    setLatestOrderStatus(order.status);

    setOrderMessage(
      `Order #${order.id} placed successfully. Status: ${order.status}.`
    );
    setCartItems([]);
    setCustomerName("");
    setDeliveryAddress("");
    setPhoneNumber("");
  } catch {
    setOrderMessage("Could not place order. Please try again.");
  } finally {
    setIsSubmitting(false);
  }
}

const cartTotal = cartItems.reduce(
  (total, item) => total + item.price * item.quantity,
  0
);

    if (isLoading) {
        return <p>Loading menu...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    return (
        <main className="app">
            <header>
                <p className="eyebrow">Food delivery</p>
                <h1>Choose your meal</h1>
                <p>Freshly prepared and delivered to your door.</p>
                <p className="cart-count">
  Cart: {cartItems.reduce((total, item) => total + item.quantity, 0)} items
</p>
            </header>

            <section className="menu-grid">
                {menuItems.map((item) => (
                    <article className="menu-card" key={item.id}>
                        <img src={item.imageUrl} alt={item.name} />
                        <div className="menu-card__content">
                            <h2>{item.name}</h2>
                            <p>{item.description}</p>
                            <strong>₹{item.price.toFixed(2)}</strong>
                            <button type="button" onClick={() => addToCart(item)}>
  Add to cart
</button>
                        </div>
                    </article>
                ))}
            </section>
            <section className="cart">
  <h2>Your cart</h2>

  {cartItems.length === 0 ? (
    <p>Your cart is empty.</p>
  ) : (
    <>
      <ul className="cart-list">
        {cartItems.map((item) => (
          <li key={item.id} className="cart-item">
            <div>
              <strong>{item.name}</strong>
              <span>₹{item.price.toFixed(2)} each</span>
            </div>

            <div className="quantity-controls">
              <button
                type="button"
                onClick={() => changeQuantity(item.id, -1)}
              >
                −
              </button>

              <span>{item.quantity}</span>

              <button
                type="button"
                onClick={() => changeQuantity(item.id, 1)}
              >
                +
              </button>
            </div>

            <strong>
              ₹{(item.price * item.quantity).toFixed(2)}
            </strong>
          </li>
        ))}
      </ul>

      <div className="cart-total">
        <span>Total</span>
        <strong>₹{cartTotal.toFixed(2)}</strong>
      </div>
    </>
  )}
</section>

<section className="checkout">
  <h2>Checkout</h2>

  <form onSubmit={placeOrder}>
    <label>
      Full name
      <input
        required
        value={customerName}
        onChange={(event) => setCustomerName(event.target.value)}
      />
    </label>

    <label>
      Delivery address
      <textarea
        required
        value={deliveryAddress}
        onChange={(event) => setDeliveryAddress(event.target.value)}
      />
    </label>

    <label>
      Phone number
      <input
        required
        type="tel"
        value={phoneNumber}
        onChange={(event) => setPhoneNumber(event.target.value)}
      />
    </label>

    <button type="submit" disabled={isSubmitting || cartItems.length === 0}>
      {isSubmitting ? "Placing order..." : "Place order"}
    </button>
  </form>

  {orderMessage && <p className="order-message">{orderMessage}</p>}
  {latestOrderId !== null && (
  <section className="order-status">
    <h3>Track order #{latestOrderId}</h3>
    <p>
      Current status: <strong>{latestOrderStatus}</strong>
    </p>
  </section>
)}
</section>
        </main>
    );
}

export default App;