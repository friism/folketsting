function handnotific() {
	if ($.cookie('notifydismissed') != 'true') {
		// arm the link
		$('div#notify a').click(function(e) {
			e.preventDefault();
			// set the cookie so that the user won't see this again
			$.cookie('notifydismissed', 'true', { expires: 365, path: '/' })
			// hide the div
			$('div#notify').slideUp('fast');
		});
		// show the notification
		$('div#notify').slideDown('slow');
	}
}