/*
* twitterCounter
* 
* Displays a counter with the remaining text. 
* 
* Example:
*   $('#description').twitterCounter(
*   {
*     limit: 140,
*     counter: '#textcounter',
*   
*     okSize: 140,
*     okStyle: '.ok',
*   
*     watchSize: 20,
*     watchStyle: '.watch',
*   
*     warningSize: 10, // replaced with 'notice' due to bug
*     warningStyle: '.warning',
*   
*     errorSize: 0,
*     errorStyle: '.error',
*   });
*
* $Version: 2008-10-24
* Copyright (c) 2008 Ali Karbassi
* ali.karbassi@gmail.com
*/
jQuery.fn.twitterCounter = function(options) {
	var curSize = $(this).val().length;
	var charsLeft = options['limit'] - curSize;
	var types = ['ok', 'watch', 'notice', 'error'];
	var x = {};

	// this parses the options into x
	$.each(types, function() {
		var el = this.toString();
		x[el] = { 'Max': options[el + 'Size'],
			'Style': options[el + 'Style'].substring(0, 1) == '.' || options[el + 'Style'].substring(0, 1) == '#' ? options[el + 'Style'].substring(1, options[el + 'Style'].length) : options[el + 'Style'],
			'Type': options[el + 'Style'].substring(0, 1) == '.' ? 'class' : 'id'
		}
	});

//	for (var i = 0; i < types.length; i++) {
//		var el = types[i].toString();

//		// Last Element check
//		if (i + 1 < types.length) {
//			var nextEl = types[i + 1].toString();
//			// console.debug(charsLeft, el, x[el]['Max'], x[nextEl]['Max']+1, charsLeft > x[nextEl]['Max'] && charsLeft < x[el]['Max'] + 1);
//			if (/*charsLeft > x[nextEl]['Max'] && */charsLeft < x[el]['Max'] + 1) {
//				clean();
//			}
//		} else {
//			// console.debug(charsLeft, el, x[el]['Max'], x[nextEl]['Max']+1, charsLeft < x[el]['Max'] + 1);
//			if (charsLeft < x[el]['Max']) {
//				clean();
//			}
//		}
//	}

	$(options['counter']).text(charsLeft);

	// Add an event so the counter updates when the user types.
	$(this).one('keyup', function() {
		$(this).update();
	});


	function update() {
		$.each(types, function(i, val) {
			
		}
	}

	function clean() {
		if (x[el]['Type'] == 'class') {
			$.each(types, function() {
				var temp = this.toString();
				if ($(options['counter']).hasClass(temp)) {
					$(options['counter']).removeClass(temp);
				}
			});
			$(options['counter']).addClass(x[el]['Style']);
		} else {
			$(options['counter']).id(x[el]['Style']);
		}
	}
};
