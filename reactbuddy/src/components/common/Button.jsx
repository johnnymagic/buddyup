import React from 'react';

/**
 * Reusable Button component with different variants and states
 * 
 * @param {Object} props - Component props
 * @param {string} [props.type='button'] - Button type (button, submit, reset)
 * @param {string} [props.size='md'] - Button size (sm, md, lg)
 * @param {boolean} [props.disabled=false] - Whether the button is disabled
 * @param {boolean} [props.fullWidth=false] - Whether the button should take full width
 * @param {Function} [props.onClick] - Click handler function
 * @param {string} [props.className] - Additional CSS classes
 * @param {React.ReactNode} props.children - Button content
 */
const Button = ({ 
  type = 'button', 
  size = 'md', 
  disabled = false, 
  fullWidth = false,
  onClick,
  className = '',
  children,
  ...rest
}) => {
  // Base classes
  const baseClasses = 'font-medium rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors';
  
  // Size classes
  const sizeClasses = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2',
    lg: 'px-6 py-3 text-lg'
  };
  
  // Width classes
  const widthClass = fullWidth ? 'w-full' : '';
  
  // Disabled classes
  const disabledClasses = disabled ? 'opacity-50 cursor-not-allowed' : '';
  
  // Combine all classes
  const combinedClasses = `${baseClasses} ${sizeClasses[size]} ${widthClass} ${disabledClasses} ${className}`;
  
  return (
    <button
      type={type}
      disabled={disabled}
      className={combinedClasses}
      onClick={onClick}
      {...rest}
    >
      {children}
    </button>
  );
};

export default Button;