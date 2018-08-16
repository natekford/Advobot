using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Advobot.Windows.Classes.Converters;
using Advobot.Windows.Classes.Validators;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A <see cref="AdvobotTextBox"/> which allows editing of a color.
	/// </summary>
	public class AdvobotColorBox : AdvobotTextBox
	{
		/// <summary>
		/// <see cref="DependencyProperty"/> for <see cref="ColorPath"/>.
		/// </summary>
		public static readonly DependencyProperty ColorPathProperty = DependencyProperty.Register(nameof(ColorPath), typeof(PropertyPath), typeof(AdvobotColorBox), new PropertyMetadata(SetColorPathProperty));
		/// <inheritdoc />
		public PropertyPath ColorPath
		{
			get => (PropertyPath)GetValue(ColorPathProperty);
			set => SetValue(ColorPathProperty, value);
		}

		//Because there isn't an way to have a template for bindings where all you do is change the path
		//And I didn't want to repeat this XAML a dozen times
		/*
		<controls:AdvobotTextBox utilities:InputBindingsManager.UpdatePropertySourceWhenEnterPressed="TextBox.Text">
			<controls:AdvobotTextBox.Text>
				<Binding Path = "Colors.HeldObject[(enums:ColorTarget)BaseBackground]"
						 Converter="{StaticResource ColorConverter}"
						 UpdateSourceTrigger="LostFocus">
					<Binding.ValidationRules>
						<validators:ColorValidator />
					</Binding.ValidationRules>
				</Binding>
			</controls:AdvobotTextBox.Text>
		</controls:AdvobotTextBox>
		*/

		/// <summary>
		/// Creates an instance of <see cref="AdvobotColorBox"/>.
		/// </summary>
		public AdvobotColorBox()
		{
			PreviewKeyDown += OnPreviewKeyDown;
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				BindingOperations.GetBindingExpression((UIElement)sender, TextProperty)?.UpdateSource();
			}
		}
		private static void SetColorPathProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var binding = new Binding
			{
				Path = (PropertyPath)e.NewValue,
				Converter = new ColorConverter(),
				UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
			};
			binding.ValidationRules.Add(new ColorValidator());
			((FrameworkElement)d).SetBinding(TextProperty, binding);
		}
	}
}