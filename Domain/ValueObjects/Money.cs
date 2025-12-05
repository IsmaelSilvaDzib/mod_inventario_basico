namespace SistemaInventario.Domain.ValueObjects
{
        public class Money : IEquatable<Money>
        {
            public decimal Value { get; private set; }

            // Constructor privado para EF Core
            private Money() { }

            // Constructor principal
            public Money(decimal value)
            {
                if (value < 0)
                    throw new ArgumentException("El precio no puede ser negativo", nameof(value));

                Value = Math.Round(value, 2); // Redondear a 2 decimales
            }

            // Operadores para facilitar el uso
            public static implicit operator decimal(Money money) => money.Value;
            public static implicit operator Money(decimal value) => new Money(value);

            // Métodos de negocio
            public Money ApplyDiscount(decimal discountPercentage)
            {
                if (discountPercentage < 0 || discountPercentage > 100)
                    throw new ArgumentException("El descuento debe estar entre 0 y 100", nameof(discountPercentage));

                var discountedValue = Value * (1 - discountPercentage / 100);
                return new Money(discountedValue);
            }

            // Implementación de IEquatable
            public bool Equals(Money other)
            {
                if (other is null) return false;
                return Value.Equals(other.Value);
            }

            public override bool Equals(object obj) => Equals(obj as Money);
            public override int GetHashCode() => Value.GetHashCode();
            public override string ToString() => Value.ToString("C"); // Formato de moneda

            // Operadores de comparación
            public static bool operator ==(Money left, Money right) => left?.Equals(right) ?? right is null;
            public static bool operator !=(Money left, Money right) => !(left == right);
        }
}
