import { Link, NavLink, Route, Routes } from 'react-router-dom';
import { ToastProvider } from './components/ToastProvider';
import ProductsPage from './pages/ProductsPage';
import ProductDetail from './pages/ProductDetail';
import ProductForm from './pages/ProductForm';
import DishesPage from './pages/DishesPage';
import DishDetail from './pages/DishDetail';
import DishForm from './pages/DishForm';

function App() {
  return (
    <ToastProvider>
      <div className="app-shell">
        <header className="app-header">
        <div>
          <h1>RecipeBook</h1>
          <p>Управление продуктами и блюдами.</p>
        </div>
        <nav>
          <NavLink to="/products">Продукты</NavLink>
          <NavLink to="/dishes">Блюда</NavLink>
        </nav>
      </header>

      <main className="app-content">
        <Routes>
          <Route
            path="/"
            element={
              <div className="home-page">
                <h2>Добро пожаловать</h2>
                <p>Используйте меню для работы с продуктами и блюдами.</p>
                <div className="home-actions">
                  <Link className="button" to="/products">
                    Продукты
                  </Link>
                  <Link className="button" to="/dishes">
                    Блюда
                  </Link>
                </div>
              </div>
            }
          />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/products/new" element={<ProductForm />} />
          <Route path="/products/:id" element={<ProductDetail />} />
          <Route path="/products/:id/edit" element={<ProductForm />} />
          <Route path="/dishes" element={<DishesPage />} />
          <Route path="/dishes/new" element={<DishForm />} />
          <Route path="/dishes/:id" element={<DishDetail />} />
          <Route path="/dishes/:id/edit" element={<DishForm />} />
        </Routes>
      </main>
      </div>
    </ToastProvider>
  );
}

export default App;
