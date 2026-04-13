import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { getProduct } from '../api';
import { ProductResponse } from '../types';
import { formatCookingOption } from '../utils';

function ProductDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState<ProductResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      return;
    }
    getProduct(Number(id))
      .then(setProduct)
      .catch(() => setError('Не удалось загрузить продукт.'));
  }, [id]);

  if (error) {
    return <div className="alert">{error}</div>;
  }

  if (!product) {
    return <div className="alert">Загрузка данных...</div>;
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>{product.name}</h2>
          <p className="small-text">ID {product.id}</p>
        </div>
        <div className="actions">
          <Link className="button secondary" to={`/products/${product.id}/edit`}>
            Редактировать
          </Link>
          <button className="button" type="button" onClick={() => navigate('/products')}>
            Назад
          </button>
        </div>
      </div>

      <div className="card">
        <p>
          <strong>Категория:</strong> {product.category}
        </p>
        <p>
          <strong>Готовность:</strong> {formatCookingOption(product.cookingRequired)}
        </p>
        <p>
          <strong>Флаги:</strong> {product.flags || 'None'}
        </p>
        <p>
          <strong>Калории / 100 г:</strong> {product.calories}
        </p>
        <p>
          <strong>Белки:</strong> {product.proteins} г / 100 г
        </p>
        <p>
          <strong>Жиры:</strong> {product.fats} г / 100 г
        </p>
        <p>
          <strong>Углеводы:</strong> {product.carbs} г / 100 г
        </p>
        <p>
          <strong>Состав:</strong> {product.composition || 'Не указан'}
        </p>
        <p>
          <strong>Создано:</strong> {product.createdAt ? new Date(product.createdAt).toLocaleString() : '—'}
        </p>
        <p>
          <strong>Обновлено:</strong> {product.updatedAt ? new Date(product.updatedAt).toLocaleString() : '—'}
        </p>
        {product.photos?.length ? (
          <div className="cards">
            <strong>Фотографии:</strong>
            <div className="image-grid">
              {product.photos.map((src, index) => (
                <img key={index} src={src} alt={`photo-${index}`} width={120} />
              ))}
            </div>
          </div>
        ) : null}
      </div>
    </div>
  );
}

export default ProductDetail;
