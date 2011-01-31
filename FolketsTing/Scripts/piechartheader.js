google.load("visualization", "1", { packages: ["piechart"] });
function drawPieChart(divid, thecols, therows, colours, scale) {
	var data = new google.visualization.DataTable(
					{ cols: thecols, rows: therows }
				);

	var thewidth = scale * 225;

	var chart = new google.visualization.PieChart(document.getElementById(divid));
	chart.draw(data, { width: thewidth, height: 225, is3D: true, legend: 'none',
		colors: colours
	});
}


//var width = 225;
//var height = 225;

//var ayedata;
//var ayecolours;

//var naydata;
//var naycolours;

//var abstaindata;
//var abstaincolours;

//var absentdata;// = new Array(<%= ViewConstants.GetVoteChartJSArray(ViewData.Model, 3) %>);
//var absentcolours;// = new Array(<%= ViewConstants.GetVoteChartJSColourArray(ViewData.Model, 3) %>)

//function initchart(divid, data, colours, scale) {
//	if(data.length > 1) {
//		if (data.length == 2 && data[0][1] == 0 && data[1][1] == 0) {
//			//charting won't handle no data
//			return; 
//		}
//		else {
//			var chart = new JSChart(divid, 'pie');
////			if(scale)
////				chart.setPieRadius(scale * width / 2.5);
////			else
////				chart.setPieRadius(width / 2.5);
//////			chart.setTitle('');
////			chart.setSize(width, height);
////			chart.setDataArray(data);
//			chart.setDataArray(new Array(['a',2],['b',3]));
////			chart.colorizePie(colours);
//			chart.draw();
//		}
//	}
//}

