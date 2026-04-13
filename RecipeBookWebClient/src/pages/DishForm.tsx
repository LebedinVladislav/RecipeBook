import { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { calculateNutritionOnServer, createDish, getDish, getProducts, updateDish } from '../api';
import { DishIngredientDto, DishCategory, DishFlags, DishFlag, ProductResponse, dishCategories, flagOptions } from '../types';
import { calculateNutrition, getAllowedDishFlags, parseDishNameMacro, isBjuSumValid, parseFlags, formatFlagLabel } from '../utils';
import { useToast } from '../components/ToastProvider';

function DishForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [photos, setPhotos] = useState<string[]>([]);
  const [photoUrl, setPhotoUrl] = useState('');
  const [portionSize, setPortionSize] = useState('100');
  const [category, setCategory] = useState<DishCategory | ''>('');
  const [selectedFlags, setSelectedFlags] = useState<DishFlag[]>([]);
  const [calories, setCalories] = useState('0');
  const [proteins, setProteins] = useState('0');
  const [fats, setFats] = useState('0');
  const [carbs, setCarbs] = useState('0');
  const [ingredients, setIngredients] = useState<DishIngredientDto[]>([]);
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [error, setError] = useState<string | null>(null);
  const { showToast } = useToast();
  const [manualCategory, setManualCategory] = useState(false);

  useEffect(() => {
    getProducts({}).then(setProducts).catch(() => null);
  }, []);

  useEffect(() => {
    if (!id) {
      return;
    }
    getDish(Number(id))
      .then((dish) => {
        setName(dish.name);
        setPhotos(dish.photos ?? []);
        setPortionSize(String(dish.portionSize));
        setCategory(dish.category || '');
        setSelectedFlags(parseFlags(dish.flags));
        setCalories(String(dish.calories));
        setProteins(String(dish.proteins));
        setFats(String(dish.fats));
        setCarbs(String(dish.carbs));
        setIngredients(dish.ingredients || []);
      })
      .catch(() => {
        setError('Не удалось загрузить блюдо.');
        showToast('Не удалось загрузить блюдо.', 'error');
      });
  }, [id]);

  useEffect(() => {
    if (!products.length || !ingredients.length) {
      return;
    }

    calculateNutritionOnServer(ingredients)
      .then((nutrition) => {
        setCalories(String(nutrition.calories));
        setProteins(String(nutrition.proteins));
        setFats(String(nutrition.fats));
        setCarbs(String(nutrition.carbs));
      })
      .catch(() => {
        const nutrition = calculateNutrition(ingredients, products);
        setCalories(String(nutrition.calories));
        setProteins(String(nutrition.proteins));
        setFats(String(nutrition.fats));
        setCarbs(String(nutrition.carbs));
      });
  }, [ingredients, products]);

  useEffect(() => {
    if (!category) {
      const parsed = parseDishNameMacro(name);
      setCategory(parsed.category || '');
    }
  }, [name, category]);

  const allowedFlags = getAllowedDishFlags(ingredients, products);

  useEffect(() => {
    setSelectedFlags((current) => current.filter((flag) => allowedFlags.includes(flag as DishFlags)));
  }, [allowedFlags]);

  const handleNameChange = (value: string) => {
    const parsed = parseDishNameMacro(value);
    if (parsed.category) {
      setName(parsed.name);
      if (!manualCategory) {
        setCategory(parsed.category);
      }
    } else {
      setName(value);
    }
  };

  const handleIngredientChange = (index: number, field: keyof DishIngredientDto, value: number) => {
    setIngredients((prev) =>
      prev.map((ingredient, currentIndex) =>
        currentIndex !== index
          ? ingredient
          : {
              ...ingredient,
              [field]: value
            }
      )
    );
  };

  const addIngredient = () => {
    const nextId = products[0]?.id ?? 0;
    setIngredients((prev) => [...prev, { productId: nextId, amount: 0 }]);
  };

  const removeIngredient = (index: number) => {
    setIngredients((prev) => prev.filter((_, currentIndex) => currentIndex !== index));
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

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    if (name.trim().length < 2) {
      const msg = 'Название должно содержать минимум 2 символа.';
      setError(msg);
      showToast(msg, 'error');
      return;
    }
    const portionSizeValue = Number(portionSize) || 0;
    const caloriesValue = Number(calories) || 0;
    const proteinsValue = Number(proteins) || 0;
    const fatsValue = Number(fats) || 0;
    const carbsValue = Number(carbs) || 0;

    if (!Number.isFinite(portionSizeValue) || portionSizeValue <= 0) {
      const msg = 'Размер порции должен быть больше нуля.';
      setError(msg);
      showToast(msg, 'error');
      return;
    }
    if (!ingredients.length) {
      const msg = 'Добавьте хотя бы один продукт в состав.';
      setError(msg);
      showToast(msg, 'error');
      return;
    }
    if (ingredients.some((item) => item.amount <= 0)) {
      const msg = 'Укажите положительный вес для каждого продукта.';
      setError(msg);
      showToast(msg, 'error');
      return;
    }
    if (!isBjuSumValid(proteinsValue, fatsValue, carbsValue)) {
      const msg = 'Сумма БЖУ не может превышать 100.';
      setError(msg);
      showToast(msg, 'error');
      return;
    }

    const request = {
      name: name.trim(),
      photos: photos.length ? photos : undefined,
      calories: caloriesValue,
      proteins: proteinsValue,
      fats: fatsValue,
      carbs: carbsValue,
      ingredients,
      portionSize: portionSizeValue,
      category: category || undefined,
      flags: selectedFlags.length ? selectedFlags : undefined
    };

    try {
      if (id) {
        await updateDish(Number(id), request);
      } else {
        await createDish(request);
      }
      navigate('/dishes');
    } catch {
      setError('Ошибка сохранения блюда. Проверьте данные и повторите.');
      showToast('Ошибка сохранения блюда. Проверьте данные и повторите.', 'error');
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>{id ? 'Редактировать блюдо' : 'Создать блюдо'}</h2>
        </div>
        <button className="button secondary" type="button" onClick={() => navigate('/dishes')}>
          Назад к списку
        </button>
      </div>

      <form className="form-grid" onSubmit={handleSubmit}>
        <div className="field">
          <label>Название</label>
          <input value={name} onChange={(event) => handleNameChange(event.target.value)} required minLength={2} />
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
                  <img src={src} alt={`Фотография блюда ${index + 1}`} />
                  <button className="button secondary photo-remove" type="button" onClick={() => removePhoto(index)}>
                    ×
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
        <div className="field">
          <label>Размер порции (г)</label>
          <input type="number" value={portionSize} onChange={(event) => setPortionSize(event.target.value)} min={1} step="0.001" required />
        </div>
        <div className="field">
          <label>Категория</label>
          <select
            value={category}
            onChange={(event) => {
              const value = event.target.value as DishCategory | '';
              setCategory(value);
              setManualCategory(!!value);
            }}
          >
            <option value="">Не выбрано</option>
            {dishCategories.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
          <p className="small-text">Макросы !десерт, !первое и т.д. автоматически назначают категорию.</p>
        </div>
        <div className="field">
          <label>Флаги блюда</label>
          <div className="checkbox-group">
            {flagOptions.filter((option) => option !== 'None').map((option) => (
              <label className="checkbox-label" key={option}>
                <input
                  type="checkbox"
                  checked={selectedFlags.includes(option as DishFlag)}
                  disabled={!allowedFlags.includes(option as DishFlags)}
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
          <p className="small-text">Флаг доступен только если все продукты в составе соответствуют ему.</p>
        </div>
        <div className="field">
          <label>Калории на порцию</label>
          <input type="number" value={calories} onChange={(event) => setCalories(event.target.value)} min={0} step="0.001" required />
        </div>
        <div className="field">
          <label>Белки на порцию</label>
          <input type="number" value={proteins} onChange={(event) => setProteins(event.target.value)} min={0} step="0.001" required />
        </div>
        <div className="field">
          <label>Жиры на порцию</label>
          <input type="number" value={fats} onChange={(event) => setFats(event.target.value)} min={0} step="0.001" required />
        </div>
        <div className="field">
          <label>Углеводы на порцию</label>
          <input type="number" value={carbs} onChange={(event) => setCarbs(event.target.value)} min={0} step="0.001" required />
        </div>

        <div className="field">
          <label>Ингредиенты</label>
          {ingredients.map((ingredient, index) => (
            <div key={index} className="card">
              <div className="form-grid">
                <div className="field">
                  <label>Продукт</label>
                  <select
                    value={ingredient.productId}
                    onChange={(event) => handleIngredientChange(index, 'productId', Number(event.target.value))}
                  >
                    {products.map((product) => (
                      <option key={product.id} value={product.id}>
                        {product.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="field">
                  <label>Вес (г)</label>
                  <input
                    type="number"
                    value={ingredient.amount}
                    onChange={(event) => handleIngredientChange(index, 'amount', Number(event.target.value))}
                    min={0}
                    step="0.001"
                  />
                </div>
                <div className="field">
                  <button type="button" className="button secondary" onClick={() => removeIngredient(index)}>
                    Удалить
                  </button>
                </div>
              </div>
            </div>
          ))}
          <button className="button" type="button" onClick={addIngredient}>
            Добавить продукт
          </button>
          {!products.length && <p className="small-text">Загрузите сначала продукты.</p>}
        </div>

        <div className="field">
          <button className="button" type="submit">
            Сохранить блюдо
          </button>
        </div>
      </form>
    </div>
  );
}

export default DishForm;
