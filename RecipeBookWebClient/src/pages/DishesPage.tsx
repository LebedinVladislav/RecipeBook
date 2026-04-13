import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getDishes, deleteDish } from '../api';
import { DishResponse, DishCategory, DishFlags, DishFlag } from '../types';
import { useToast } from '../components/ToastProvider';
import { formatFlagLabel, formatFlags } from '../utils';

const categories: DishCategory[] = ['Десерт', 'Первое', 'Второе', 'Напиток', 'Салат', 'Суп', 'Перекус'];
const flags: DishFlags[] = ['None', 'Веган', 'БезГлютена', 'БезСахара'];

function DishesPage() {
  const [dishes, setDishes] = useState<DishResponse[]>([]);
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState<DishCategory | ''>('');
  const [selectedFlags, setSelectedFlags] = useState<DishFlag[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { showToast } = useToast();

  useEffect(() => {
    setLoading(true);
    setError(null);
    const flagsParam = selectedFlags.length > 0 ? selectedFlags.join(', ') : undefined;
    getDishes({
      category: category || undefined,
      flags: (flagsParam as DishFlags) || undefined,
      search: search || undefined
    })
      .then(setDishes)
      .catch(() => {
        setError('Не удалось загрузить блюда.');
        showToast('Не удалось загрузить блюда.', 'error');
      })
      .finally(() => setLoading(false));
  }, [search, category, selectedFlags]);

  const handleDelete = async (id: number) => {
    if (!window.confirm('Удалить блюдо?')) {
      return;
    }
    try {
      await deleteDish(id);
      setDishes((prev) => prev.filter((item) => item.id !== id));
    } catch {
      showToast('Ошибка удаления блюда.', 'error');
    }
  };

  return (
    <div>
      <div className="page-header">
        <h2>Блюда</h2>
        <Link className="button" to="/dishes/new">
          Создать блюдо
        </Link>
      </div>

      <div className="form-grid">
        <div className="field">
          <label>Поиск</label>
          <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Название блюда" />
        </div>
        <div className="field">
          <label>Категория</label>
          <select value={category} onChange={(event) => setCategory(event.target.value as DishCategory | '')}>
            <option value="">Все</option>
            {categories.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label>Флаги</label>
          <div className="checkbox-group">
            {flags.filter((option) => option !== 'None').map((option) => (
              <label className="checkbox-label" key={option}>
                <input
                  type="checkbox"
                  checked={selectedFlags.includes(option as DishFlag)}
                  onChange={() => {
                    const flag = option as DishFlag;
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
      </div>

      {loading && <div className="alert">Загрузка блюд...</div>}
      {error && <div className="alert">{error}</div>}

      {!loading && dishes.length === 0 && (
        <div className="alert">Нет блюд.</div>
      )}

      <div className="products-grid">
        {dishes.map((dish) => (
          <div key={dish.id} className="product-card">
            <Link to={`/dishes/${dish.id}`} className="product-card-link">
              <div className="product-card-image">
                {dish.photos?.[0] ? (
                  <img src={dish.photos[0]} alt={dish.name} />
                ) : (
                  <div className="product-card-placeholder">Нет фото</div>
                )}
              </div>
              <div className="product-card-content">
                <h3 className="product-card-title">{dish.name}</h3>
                <div className="product-card-meta">
                  <div className="meta-item">
                    <span className="meta-label">Категория:</span>
                    <span className="meta-value">{dish.category || 'Не указана'}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Флаги:</span>
                    <span className="meta-value">{dish.flags && dish.flags !== 'None' ? formatFlags(dish.flags) : 'нет'}</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Порция:</span>
                    <span className="meta-value">{dish.portionSize}г</span>
                  </div>
                  <div className="meta-item">
                    <span className="meta-label">Ингредиентов:</span>
                    <span className="meta-value">{dish.ingredients?.length || 0}</span>
                  </div>
                </div>
                <div className="product-card-nutrition">
                  <div className="nutrition-item">
                    <span className="nutrition-label">Калории</span>
                    <span className="nutrition-value">{dish.calories}</span>
                  </div>
                  <div className="nutrition-item">
                    <span className="nutrition-label">Б</span>
                    <span className="nutrition-value">{dish.proteins}г</span>
                  </div>
                  <div className="nutrition-item">
                    <span className="nutrition-label">Ж</span>
                    <span className="nutrition-value">{dish.fats}г</span>
                  </div>
                  <div className="nutrition-item">
                    <span className="nutrition-label">У</span>
                    <span className="nutrition-value">{dish.carbs}г</span>
                  </div>
                </div>
              </div>
            </Link>
            <div className="product-card-actions">
              <Link className="button" to={`/dishes/${dish.id}/edit`}>
                Редактировать
              </Link>
              <button className="button secondary" type="button" onClick={() => handleDelete(dish.id)}>
                Удалить
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default DishesPage;
