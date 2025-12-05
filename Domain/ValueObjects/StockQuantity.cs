namespace SistemaInventario.Domain.ValueObjects
{
        public class StockQuantity : IEquatable<StockQuantity>
        {
            public int Value { get; private set; }

            // Constructor privado para EF Core
            private StockQuantity() { }

            // Constructor principal
            public StockQuantity(int value)
            {
                if (value < 0)
                    throw new ArgumentException("La cantidad en stock no puede ser negativa", nameof(value));

                Value = value;
            }

            // Operadores para facilitar el uso
            public static implicit operator int(StockQuantity stock) => stock.Value;
            public static implicit operator StockQuantity(int value) => new StockQuantity(value);

            // Métodos de negocio
            public StockQuantity Add(int quantity)
            {
                if (quantity <= 0)
                    throw new ArgumentException("La cantidad a agregar debe ser positiva", nameof(quantity));

                return new StockQuantity(Value + quantity);
            }

            public StockQuantity Subtract(int quantity)
            {
                if (quantity <= 0)
                    throw new ArgumentException("La cantidad a sustraer debe ser positiva", nameof(quantity));

                if (Value - quantity < 0)
                    throw new InvalidOperationException("No se puede tener stock negativo");

                return new StockQuantity(Value - quantity);
            }

            public bool IsLow() => Value < 10;
            public bool IsEmpty() => Value == 0;

            // Implementación de IEquatable
            public bool Equals(StockQuantity other)
            {
                if (other is null) return false;
                return Value.Equals(other.Value);
            }

            public override bool Equals(object obj) => Equals(obj as StockQuantity);
            public override int GetHashCode() => Value.GetHashCode();
            public override string ToString() => Value.ToString();

            // Operadores de comparación
            public static bool operator ==(StockQuantity left, StockQuantity right) => left?.Equals(right) ?? right is null;
            public static bool operator !=(StockQuantity left, StockQuantity right) => !(left == right);
        }
}
