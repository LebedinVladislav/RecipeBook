import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getDish, getProducts } from '../api';
import { DishResponse, ProductResponse } from '../types';

function DishDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [dish, setDish] = useState<DishResponse | null>(null);
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      return;
    }
    getDish(Number(id))
      .then(setDish)
      .catch(() => setError('Не удалось загрузить блюдо.'));
    getProducts({}).then(setProducts).catch(() => null);
  }, [id]);

  if (error) {
    return <div className="alert">{error}</div>;
  }

  if (!dish) {
    return <div className="alert">Загрузка блюда...</div>;
  }

  const ingredientRows = dish.ingredients?.map((ingredient) => {
    const product = products.find((item) => item.id === ingredient.productId);
    return {
      id: ingredient.productId,
      name: product?.name || `Продукт #${ingredient.productId}`,
      amount: ingredient.amount
    };
  }) ?? [];

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>{dish.name}</h2>
          <p className="small-text">ID {dish.id}</p>
        </div>
        <div className="actions">
          <button className="button secondary" type="button" onClick={() => navigate('/dishes')}>
            Назад
          </button>
        </div>
      </div>

      <div className="card">
        <p>
          <strong>Категория:</strong> {dish.category || 'Не указана'}
        </p>
        <p>
          <strong>Флаги:</strong> {dish.flags || 'None'}
        </p>
        <p>
          <strong>Дата создания:</strong> {new Date(dish.createdAt).toLocaleString()}
        </p>
        <p>
          <strong>Дата редактирования:</strong> {new Date(dish.updatedAt).toLocaleString()}
        </p>
        <p>
          <strong>Размер порции:</strong> {dish.portionSize} г
        </p>
        {dish.photos?.length ? (
          <div className="cards">
            <strong>Фотографии:</strong>
            <div className="image-grid">
              {dish.photos.map((src, index) => (
                <img key={index} src={src} alt={`photo-${index}`} width={120} />
              ))}
            </div>
          </div>
        ) : null}
        <p>
          <strong>Калории:</strong> {dish.calories}
        </p>
        <p>
          <strong>Белки:</strong> {dish.proteins} г
        </p>
        <p>
          <strong>Жиры:</strong> {dish.fats} г
        </p>
        <p>
          <strong>Углеводы:</strong> {dish.carbs} г
        </p>
        <div>
          <strong>Состав:</strong>
          <ul>
            {ingredientRows.map((ingredient) => (
              <li key={ingredient.id}>
                {ingredient.name}: {ingredient.amount} г
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}

export default DishDetail;
