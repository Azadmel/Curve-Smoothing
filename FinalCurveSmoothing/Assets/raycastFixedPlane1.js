#pragma strict

var distanceThreshold : float = 0.25;
var lineColor : Color;

private var LR : LineRenderer;
private var drawing : boolean = false;

// array used to maintain accessible list of points for linerenderer
private var pointList = new Array();

var gobj : GameObject ;
public var csScript : SmoothingScript ;
public var threshold:float;
public var Strokewidth:float;
var pointarray : Vector3[]; 

public function Awake()
{
	//Get the CSharp Script
	gobj =  GameObject.Find("Main Camera");
	if(gobj!=null){
		
		csScript = gobj.GetComponent("SmoothingScript") ;
		if(csScript!=null){
			Debug.Log("scriiipt" + csScript.GetType());
		}
	}
	else Debug.Log("null");
//Don't forget to place the CSharp script you want to call inside the 'Standard Assets' folder
}

function Update () {

	if (Input.GetMouseButtonDown(0)) {
		var hit : RaycastHit;
		if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition),  hit, 1000)) {
		
			// create new gameObject with lineRenderer
			var LRobj = new GameObject("lineObj");
			LR = LRobj.AddComponent(LineRenderer);
			LR.material = new Material (Shader.Find("VertexLit"));
			LR.material.SetColor("_Emission", lineColor);
			LR.SetWidth(0.125, 0.125);
			
			// set position of lineRenderer points
			LR.SetPosition(0, hit.point);   
			LR.SetPosition(1, hit.point);
			
			// add the two points to the array
			pointList = new Array();
			pointList.Add(hit.point);  
			pointList.Add(hit.point);     
		
			drawing = true;

		}
	}
	
	
	if (Input.GetMouseButton(0) && drawing) {

		if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition),  hit, 1000)) {
			
			Debug.DrawLine(transform.position, hit.point ) ;
			
			//update the last point of the linerenderer to follow the mouse
			LR.SetPosition(pointList.length-1, hit.point);
			
			// check if point is past the distance threshold from the previous point in the array
			if (Vector3.Distance( hit.point, pointList[pointList.length-2]) > distanceThreshold ) {
				// add a new point
				LR.SetVertexCount(pointList.length+1);
				// set the last point in the array to the current position
				pointList[pointList.length-1] = hit.point;
				// add a new poinnt to the array
				pointList.Add(hit.point); 
				// update position before redraw
				LR.SetPosition(pointList.length-1, hit.point);		
			}
	}
		
	
	}
	if (Input.GetMouseButtonUp(0) && drawing) {
	
		// call point list smoothing/reduction function here
		
		drawing = false;
		// The DrawSmoothFunction can only accept .NET built in array type, and not the Javascript array object. 	
		pointarray = pointList.ToBuiltin(Vector3) as Vector3[];
		csScript.DrawSmoothCurve(pointarray,threshold,Strokewidth);
	}
	


}

