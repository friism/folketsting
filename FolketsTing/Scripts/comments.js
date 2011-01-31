function init_effects() {
	// this handles mouseovering of relevant elements
	$("div.commentable p, div.commentable ul, div.commentable ol").bind("mouseenter", function(e) {
		// this handles background greying
		$(this).siblings().andSelf().addClass("over");

		//this displays the comment link
		$(this).siblings().andSelf().find("span.commentcontrol").show();
	});

	$("div.commentable p, div.commentable ul, div.commentable ol").bind("mouseleave", function(e) {
		// this handles background greying
		$(this).siblings().andSelf().removeClass("over");

		//this displays the comment link
		$(this).siblings().andSelf().find("span.commentcontrol").hide();
	});

	// get dialog going
	$('#authdialog').dialog({ autoOpen: false, modal: true, buttons: {}, resizable: false });
	// enable close link
	$('#closedialog').click(function(e) { e.preventDefault(); $('#authdialog').dialog('close') });
}

function post_vote(e, element, thevote) {
	// get id an type of comment
	var id = element.parents('div.comment').attr('id');
	// the voteurl used here is set elsewhere
	$.post(voteurl,
		{ commentid: id, vote: thevote },
		function() {
			// reload the damn comments
			element.parents('div.commentable').find('a.js:first').click().click();
		}
	);
}

function arm_votelinks() {
	$('div.commentcontrol a.yes').click(function(e) {
		e.preventDefault();
		post_vote(e, $(this), 1, voteurl);
	});
	$('div.commentcontrol a.no').click(function(e) {
		e.preventDefault();
		post_vote(e, $(this), 0, voteurl);
	});
	$('div.commentcontrol a.complain').click(function(e) {
		e.preventDefault();
		alert('funktionen er på vej');
	});
}

function on_comment_submitted(element_id, element_type, data) {
	//	var comment_box = $('div#' + element_id + ' a.js:first')
	var element_class;
	switch (element_type) {
		case 'Question': element_class = 'question'; break;
		case 'QuestionBackground': element_class = 'background'; break;
		case 'Change': element_class = 'lawchange'; break;
		case 'Section': element_class = 'section'; break;
		case 'Speech': element_class = 'speech'; break;
		case 'Answer': element_class = 'answer'; break;
		default: alert('unknown element_type');
	}
	var comment_box = $('div.' + element_id + '.'+ element_class + ' a.js:first')
	comment_box.click().click();
	if (data != "false") {
		publishCommentFeed(data);
	};
}

var feed_published = false;
function publishCommentFeed(data) {
	FB.init("32ea3676df27273d35d2dccf64b22fe5", "/xd_receiver.htm");
	FB.ensureInit(function() {
		FB.Connect.requireSession(function() {
			feedDialog(data);
		});
	});
}

function feedDialog(data) {
	FB.Connect.showFeedDialog(data.content.feed.template_id, data.content.feed.template_data, null, "", null, FB.RequireConnect.prompConnect, function() {
		feed_published = true;
	});
	if (feed_published == false) {
		setTimeout(function() {
			feedDialog(data);
		}, 2000)
	};
}

function publishCommentFeed2(data) {
	FB.init("32ea3676df27273d35d2dccf64b22fe5", "/xd_receiver.htm");
	FB.ensureInit(function() {
		FB.Connect.showFeedDialog(
		data["content"]["feed"]["template_id"],
		data["content"]["feed"]["template_data"],
		null, "", null, FB.RequireConnect.promptConnect);
	});
}

function commentable_click(e, element, loadingurl, actionurl, commenttype, returnurl, width) {
	var speechdiv = element.parents("div.commentable");
	if (speechdiv.find("div.content > p,div.content > ul, div.content ol").hasClass('clicked')) {
		//hide comments and clean up
		speechdiv.find("div.content > p, div.content > ul, div.content ol").removeClass("clicked");
		speechdiv.find('span a.js').html('Kommentarer');
		var commdiv = speechdiv.children("div.content").children('div.comments');
		commdiv.hide('fast', function() { commdiv.remove(); });
	}
	else {
		// make the p yellow
		speechdiv.find("div.content > p, div.content > ul, div.content ol").addClass("clicked");

		// build a div with a loading anim
		var thewidth = width ? width : 19; // default to 19
		$("<div>")
					.addClass("comments span-" + thewidth)
					.attr("style", "display: none")
					.append(
						$('<img class="loading">')
							.attr("src", loadingurl)
							.attr("alt", "loading")
					)
					.appendTo(speechdiv.children("div.content"));

		// mark the div were we'll do the inserting after the call
		speechdiv.toggleClass("inserthere");

		// this is very hacky, the id is the css class that's a number
		var classes = speechdiv.attr("class").split(' ');
		var id;
		$.each(classes, function(i, val) {
			if (/\d+/.test(val)) {
				id = val;
			}
		});

		$.ajax({
			type: "GET",
			url: actionurl + id + '/' + commenttype + '?returnurl=' + returnurl,
			dataType: 'html',
			success: function(data) {
				// kill the loading img
				$('img.loading').remove();
				// change the link text
				speechdiv.find('span a.js').html('Skjul kommentarer');

				var thediv = $('div.inserthere div.comments');
				$('div.inserthere').toggleClass('inserthere');
				thediv.html(data);
				thediv.slideDown('fast');

				// key up the newly added links, great uglyness here
				if (isauthed) {
					thediv.find('a.respond').click(function(e) {
						e.preventDefault();
						$(this).parent('div.commentcontrol').siblings('div.respond').slideDown('fast');
					});
				}
				else {
					thediv.find('a.respond').click(function(e) {
						e.preventDefault();
						$('#authdialog').dialog('open');
					});
				}

				thediv.find('a.hidelink').click(function(e) {
					e.preventDefault();
					$(this).parents('div.respond').slideUp('fast');

				});
				arm_votelinks(voteurl);
			},
			error: function() {
				$('img.loading').remove();
				$('div.inserthere').toggleClass('inserthere');
			}
		});
	}
}

function respond_click(e, element) {
	element.parent('div.commentcontrol').siblings('div.respond').slideDown('fast');
}