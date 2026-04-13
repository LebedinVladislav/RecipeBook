import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import { getProducts, deleteProduct } from '../api';
import { ProductResponse, ProductCategory, CookingRequired, ProductFlags, ProductFlag } from '../types';
import { useToast } from '../components/ToastProvider';
import { formatCookingOption, formatFlagLabel, formatFlags } from '../utils';

const categories: ProductCategory[] = [
  'Замороженный',
  'Мясной',
  'Овощи',
  'Зелень',
  'Специи',
  'Крупы',
  'Консервы',
  'Жидкость',
  'Сладости'
];

const cookingOptions: CookingRequired[] = [
  'ГотовыйКУпотреблению',
  'Полуфабрикат',
  'ТребуетПриготовления'
];

const sortOptions = ['Название', 'Калории', 'Белки', 'Жиры', 'Углеводы'] as const;
const sortApiMap: Record<string, 'Name' | 'Calories' | 'Proteins' | 'Fats' | 'Carbs'> = {
  'Название': 'Name',
  'Калории': 'Calories',
  'Белки': 'Proteins',
  'Жиры': 'Fats',
  'Углеводы': 'Carbs'
};
const flagOptions: ProductFlags[] = ['None', 'Веган', 'БезГлютена', 'БезСахара'];

function ProductsPage() {
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState<ProductCategory | ''>('');
  const [cookingRequired, setCookingRequired] = useState<CookingRequired | ''>('');
  const [selectedFlags, setSelectedFlags] = useState<ProductFlag[]>([]);
  const [sortBy, setSortBy] = useState<typeof sortOptions[number] | ''>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { showToast } = useToast();

  useEffect(() => {
    setLoading(true);
    setError(null);
    const flagsParam = selectedFlags.length > 0 ? selectedFlags.join(', ') : undefined;
    const apiSortBy = sortBy ? sortApiMap[sortBy] : undefined;
    getProducts({
      category: category || undefined,
      cookingRequired: cookingRequired || undefined,
      flags: (flagsParam as ProductFlags) || undefined,
      search: search || undefined,
      sortBy: apiSortBy
    })
      .then(setProducts)
      .catch(() => {
        setError('Не удалось загрузить продукты.');
        showToast('Не удалось загрузить продукты.', 'error');
      })
      .finally(() => setLoading(false));
  }, [search, category, cookingRequired, selectedFlags, sortBy]);

  const handleDelete = async (id: number) => {
    if (!window.confirm('Удалить продукт?')) {
      return;
    }
    try {
      await deleteProduct(id);
      setProducts((prev) => prev.filter((item) => item.id !== id));
    } catch (error) {
      const rawMessage = (error as any)?.response?.data;
      const message = typeof rawMessage === 'string'
        ? rawMessage
        : rawMessage?.Message || 'Ошибка удаления. Возможно, продукт используется в блюде.';
      showToast(message, 'error');
    }
  };

  return (
    <div>
      <div className="page-header">
        <h2>Продукты</h2>
        <Link className="button" to="/products/new">
          Создать продукт
        </Link>
      </div>

      <div className="form-grid">
        <div className="field">
          <label>Поиск</label>
          <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Название продукта" />
        </div>
        <div className="field">
          <label>Категория</label>
          <select value={category} onChange={(event) => setCategory(event.target.value as ProductCategory | '')}>
            <option value="">Все</option>
            {categories.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label>Необходимость готовки</label>
          <select value={cookingRequired} onChange={(event) => setCookingRequired(event.target.value as CookingRequired | '')}>
            <option value="">Все</option>
            {cookingOptions.map((option) => (
              <option key={option} value={option}>
                {formatCookingOption(option)}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label>Флаги</label>
          <div className="checkbox-group">
            {flagOptions.filter((option) => option !== 'None').map((option) => (
              <label className="checkbox-label" key={option}>
                <input
                  type="checkbox"
                  checked={selectedFlags.includes(option as ProductFlag)}
                  onChange={() => {
                    const flag = option as ProductFlag;
                    setSelectedFlags((current) =>
                      current.includes(flag) ? current.filter((item) => item !== flag) : [...current, flag]
                    );
                  }}
                />
                {formatFlagLabel(option)}
              </label>
            ))}
          </div>
        </div>
        <div className="field">
          <label>Сортировка</label>
          <select value={sortBy} onChange={(event) => setSortBy(event.target.value as typeof sortOptions[number] | '')}>
            <option value="">Без сортировки</option>
            {sortOptions.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </div>
      </div>

      {loading && <div className="alert">Загрузка продуктов...</div>}
      {error && <div className="alert">{error}</div>}

      {!loading && products.length === 0 && (
        <div className="alert">Нет продуктов.</div>
      )}

      <div className="products-grid">
        {products.map((product) => (
          <div key={product.id} className="product-card">
            <Link to={`/products/${product.id}`} className="product-card-link">
              <div className="product-card-image">
                {product.photos?.[0] ? (
                  <img src={product.photos[0]} alt={product.name} />
                ) : (
                  <div className="product-card-placeholder">Нет фото</div>
                )}
              </div>
              <div className="product-card-content">
                <h3 className="product-card-title">{product.name}</h3>
                <div className="product-card-meta">
                  <div className="meta-item">
                    <span className="meta-label">Категория:</span>
                    <span className="meta-value">{product.category}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Готовность:</span>
                    <span className="meta-value">{formatCookingOption(product.cookingRequired)}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Флаги:</span>
                    <span className="meta-value">{product.flags && product.flags !== 'None' ? formatFlags(product.flags) : 'Нет'}</span>
                  </div>
                </div>
                <div className="product-card-nutrition">
                  <div className="nutrition-item">
                    <span className="nutrition-label">Калории</span>
                    <span className="nutrition-value">{product.calories}</span>
                  </div>
                  <div className="nutrition-item">
                    <span className="nutrition-label">Б</span>
                    <span className="nutrition-value">{product.proteins}г</span>
                  </div>
                  <div className="nutrition-item">
                    <span className="nutrition-label">Ж</span>
                    <span className="nutrition-value">{product.fats}г</span>
                  </div>
                  <div className="nutrition-item">
                    <span className="nutrition-label">У</span>
                    <span className="nutrition-value">{product.carbs}г</span>
                  </div>
                </div>
              </div>
            </Link>
            <div className="product-card-actions">
              <Link className="button" to={`/products/${product.id}/edit`}>
                Редактировать
              </Link>
              <button className="button secondary" type="button" onClick={() => handleDelete(product.id)}>
                Удалить
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default ProductsPage;
