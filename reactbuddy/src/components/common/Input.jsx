import React from 'react';

/**
 * Reusable Input component with various input types and states
 * 
 * @param {Object} props - Component props
 * @param {string} [props.type='text'] - Input type (text, email, password, number, etc.)
 * @param {string} [props.id] - Input id attribute
 * @param {string} [props.name] - Input name attribute
 * @param {string} [props.value] - Input value
 * @param {Function} [props.onChange] - Change handler function
 * @param {string} [props.placeholder] - Input placeholder text
 * @param {boolean} [props.disabled=false] - Whether the input is disabled
 * @param {boolean} [props.required=false] - Whether the input is required
 * @param {boolean} [props.error=false] - Whether the input has an error
 * @param {string} [props.errorMessage] - Error message to display
 * @param {string} [props.className] - Additional CSS classes
 */
const Input = ({
  type = 'text',
  id,
  name,
  value,
  onChange,
  placeholder,
  disabled = false,
  required = false,
  error = false,
  errorMessage,
  className = '',
  ...rest
}) => {
  // Base classes
  const baseClasses = 'px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 w-full';
  
  // State classes
  const stateClasses = error
    ? 'border-red-300 text-red-900 placeholder-red-300 focus:border-red-500 focus:ring-red-500'
    : disabled
    ? 'border-gray-200 bg-gray-100 text-gray-500 cursor-not-allowed'
    : 'border-gray-300 focus:border-blue-500';
  
  // Combine all classes
  const combinedClasses = `${baseClasses} ${stateClasses} ${className}`;
  
  return (
    <div className="w-full">
      <input
        type={type}
        id={id}
        name={name}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        disabled={disabled}
        required={required}
        className={combinedClasses}
        aria-invalid={error ? 'true' : 'false'}
        aria-describedby={error && errorMessage ? `${id}-error` : undefined}
        {...rest}
      />
      {error && errorMessage && (
        <p className="mt-1 text-sm text-red-600" id={`${id}-error`}>
          {errorMessage}
        </p>
      )}
    </div>
  );
};

export default Input;