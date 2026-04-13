import { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { createProduct, getProduct, updateProduct } from '../api';
import { productCategories, cookingOptions, flagOptions, ProductRequest, ProductCategory, CookingRequired, ProductFlag } from '../types';
import { isBjuSumValid, parseFlags, formatFlagLabel, formatCookingOption } from '../utils';
import { useToast } from '../components/ToastProvider';

function ProductForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [photos, setPhotos] = useState<string[]>([]);
  const [photoUrl, setPhotoUrl] = useState('');
  const [calories, setCalories] = useState('');
  const [proteins, setProteins] = useState('');
  const [fats, setFats] = useState('');
  const [carbs, setCarbs] = useState('');
  const [composition, setComposition] = useState('');
  const [category, setCategory] = useState<ProductCategory>(productCategories[0]);
  const [cookingRequired, setCookingRequired] = useState<CookingRequired>(cookingOptions[0]);
  const [selectedFlags, setSelectedFlags] = useState<ProductFlag[]>([]);
  const [error, setError] = useState<string | null>(null);
  const { showToast } = useToast();

  useEffect(() => {
    if (!id) {
      return;
    }

    getProduct(Number(id))
      .then((product) => {
        setName(product.name);
        setPhotos(product.photos ?? []);
        setCalories(String(product.calories));
        setProteins(String(product.proteins));
        setFats(String(product.fats));
        setCarbs(String(product.carbs));
        setComposition(product.composition || '');
        setCategory(product.category);
        setCookingRequired(product.cookingRequired);
        setSelectedFlags(parseFlags(product.flags));
      })
      .catch(() => {
        setError('Не удалось загрузить продукт.');
        showToast('Не удалось загрузить продукт.', 'error');
      });
  }, [id]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    if (name.trim().length < 2) {
      const msg = 'Название должно содержать минимум 2 символа.';
      setError(msg);
      showToast(msg, 'error');
      return;
    }
    const caloriesValue = Number(calories) || 0;
    const proteinsValue = Number(proteins) || 0;
    const fatsValue = Number(fats) || 0;
    const carbsValue = Number(carbs) || 0;

    if (!isBjuSumValid(proteinsValue, fatsValue, carbsValue)) {
      const msg = 'Сумма БЖУ на 100 г не может превышать 100.';
      setError(msg);
      showToast(msg, 'error');
      return;
    }
    if (caloriesValue < 0 || proteinsValue < 0 || fatsValue < 0 || carbsValue < 0) {
      const msg = 'КБЖУ не может быть отрицательным.';
      setError(msg);
      showToast(msg, 'error');
      return;
    }

    const product: ProductRequest = {
      name: name.trim(),
      photos: photos.length ? photos : undefined,
      calories: caloriesValue,
      proteins: proteinsValue,
      fats: fatsValue,
      carbs: carbsValue,
      composition: composition.trim() || undefined,
      category,
      cookingRequired,
      flags: selectedFlags.length ? selectedFlags : undefined
    };

    try {
      if (id) {
        await updateProduct(Number(id), product);
      } else {
        await createProduct(product);
      }
      navigate('/products');
    } catch {
      setError('Ошибка сохранения продукта. Проверьте данные и повторите.');
      showToast('Ошибка сохранения продукта. Проверьте данные и повторите.', 'error');
    }
  };

  const handlePhotoFileChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files ? Array.from(event.target.files) : [];
    if (!files.length) {
      return;
    }

    const availableSlots = 5 - photos.length;
    if (availableSlots <= 0) {
      showToast('Максимум 5 фотографий.', 'error');
      return;
    }

    const selectedFiles = files.slice(0, availableSlots);
    const loadedPhotos: string[] = [];

    await Promise.all(
      selectedFiles.map(
        (file) =>
          new Promise<void>((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => {
              loadedPhotos.push(reader.result as string);
              resolve();
            };
            reader.onerror = () => {
              reject(new Error('Не удалось загрузить фото.'));
            };
            reader.readAsDataURL(file);
          })
      )
    ).catch(() => {
      showToast('Не удалось загрузить одно или несколько фото.', 'error');
    });

    if (loadedPhotos.length) {
      setPhotos((current) => [...current, ...loadedPhotos]);
    }
    if (event.target) {
      event.target.value = '';
    }
  };

  const addPhotoUrl = () => {
    const trimmed = photoUrl.trim();
    if (!trimmed) {
      return;
    }
    if (photos.length >= 5) {
      showToast('Максимум 5 фотографий.', 'error');
      return;
    }

    setPhotos((current) => [...current, trimmed]);
    setPhotoUrl('');
  };

  const removePhoto = (index: number) => {
    setPhotos((current) => current.filter((_, currentIndex) => currentIndex !== index));
  };

  const toggleFlag = (flag: ProductFlag) => {
    setSelectedFlags((current) =>
      current.includes(flag) ? current.filter((item) => item !== flag) : [...current, flag]
    );
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>{id ? 'Редактировать продукт' : 'Создать продукт'}</h2>
        </div>
        <button className="button secondary" type="button" onClick={() => navigate('/products')}>
          Назад к списку
        </button>
      </div>

      <form className="form-grid" onSubmit={handleSubmit}>
        <div className="field">
          <label>Название</label>
          <input value={name} onChange={(event) => setName(event.target.value)} required minLength={2} />
        </div>
        <div className="field">
          <label>Фотографии</label>
          <input type="file" accept="image/*" multiple onChange={handlePhotoFileChange} />
          <p className="small-text">Загрузите до 5 изображений или добавьте ссылки.</p>
          <div className="photo-url-input">
            <input
              value={photoUrl}
              onChange={(event) => setPhotoUrl(event.target.value)}
              placeholder="URL фотографии"
            />
            <button className="button secondary" type="button" onClick={addPhotoUrl}>
              Добавить
            </button>
          </div>
          {photos.length > 0 && (
            <div className="photo-preview-list">
              {photos.map((src, index) => (
                <div className="photo-preview" key={`${src}-${index}`}>
                  <img src={src} alt={`Фотография продукта ${index + 1}`} />
                  <button className="button secondary photo-remove" type="button" onClick={() => removePhoto(index)}>
                    ×
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
        <div className="field">
          <label>Калорийность на 100 г</label>
          <input type="number" value={calories} onChange={(event) => setCalories(event.target.value)} min={0} step="0.001" required />
        </div>
        <div className="field">
          <label>Белки на 100 г</label>
          <input type="number" value={proteins} onChange={(event) => setProteins(event.target.value)} min={0} max={100} step="0.001" required />
        </div>
        <div className="field">
          <label>Жиры на 100 г</label>
          <input type="number" value={fats} onChange={(event) => setFats(event.target.value)} min={0} max={100} step="0.001" required />
        </div>
        <div className="field">
          <label>Углеводы на 100 г</label>
          <input type="number" value={carbs} onChange={(event) => setCarbs(event.target.value)} min={0} max={100} step="0.001" required />
        </div>
        <div className="field">
          <label>Состав</label>
          <textarea value={composition} onChange={(event) => setComposition(event.target.value)} />
        </div>
        <div className="field">
          <label>Категория</label>
          <select value={category} onChange={(event) => setCategory(event.target.value as ProductCategory)}>
            {productCategories.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label>Необходимость готовки</label>
          <select value={cookingRequired} onChange={(event) => setCookingRequired(event.target.value as CookingRequired)}>
            {cookingOptions.map((option) => (
              <option key={option} value={option}>
                {formatCookingOption(option)}
              </option>
            ))}
          </select>
        </div>
        <div className="field">
          <label>Флаги продукта</label>
          <div className="checkbox-group">
            {flagOptions.filter((option) => option !== 'None').map((option) => (
              <label className="checkbox-label" key={option}>
                <input
                  type="checkbox"
                  checked={selectedFlags.includes(option as ProductFlag)}
                  onChange={() => toggleFlag(option as ProductFlag)}
                />
                {option}
              </label>
            ))}
          </div>
        </div>
        <div className="field">
          <button className="button" type="submit">
            Сохранить
          </button>
        </div>
      </form>
    </div>
  );
}

export default ProductForm;
