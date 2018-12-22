/// <reference path="jquery-3.3.1.intellisense.js" />
ko.bindingHandlers.withProperties = {
	init: function (element, valueAccessor, allBindings, viewModel, bindingContext)
	{
		// Make a modified binding context, with a extra properties, and apply it to descendant elements
		var innerBindingContext = bindingContext.extend(valueAccessor);
		ko.applyBindingsToDescendants(innerBindingContext, element);

		// Also tell KO *not* to bind the descendants itself, otherwise they will be bound twice
		return { controlsDescendantBindings: true };
	}
};
ko.virtualElements.allowedBindings.withProperties = true;

ko.bindingHandlers.dialog = {
	defaultValues: {
		flat: true,
		draggable: true,
		overlay: true,
		shadow: true,
		padding: 10
	},
	init: function (element, valueAccessor, allBindings, viewModel, bindingContext)
	{
		$(element).click(function ()
		{
			var options = ko.utils.extend(ko.bindingHandlers.dialog.defaultValues, ko.unwrap(valueAccessor()));
			var innerBindingContext = null;

			if (options.data)
			{
				var extendData = options.data;
				if (ko.isObservable(extendData))
					extendData = ko.unwrap(extendData);
				else if (typeof extendData === "function")
					extendData = extendData(bindingContext);

				innerBindingContext = bindingContext.createChildContext(
					extendData,
					null, // Optionally, pass a string here as an alias for the data item in descendant contexts
					function (context)
					{
						//ko.utils.extend(context, valueAccessor());
					});

				options.onShow = function (newElement)
				{
					if (ko.isObservable(extendData.$visible))
					{
						extendData.$visible.subscribe(function (val)
						{
							if (val === false)
								$.Dialog.close();
						});
					}

					ko.applyBindingsToDescendants(innerBindingContext, newElement[0]);
				};
			}
			if (options.childTemplate)
			{
				options.content = document.getElementById(options.childTemplate).text;
			}

			$.Dialog(options);

			if (innerBindingContext)
			{
				// Also tell KO *not* to bind the descendants itself, otherwise they will be bound twice
				return { controlsDescendantBindings: true };
			}
		});
	}
};

ko.bindingHandlers.noop = {
	init: function (element, valueAccessor, allBindings, viewModel, bindingContext)
	{
		ko.unwrap(valueAccessor());
	}
};
