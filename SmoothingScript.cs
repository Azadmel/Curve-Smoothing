using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

public class SmoothingScript : MonoBehaviour {
	
	float distanceThreshold;
	Color lineColor,col1;
	RaycastHit hit;
	GameObject LRobj;
	LineRenderer LR;
	
	List<Vector3> pointList;
	GameObject CsObj;
	MonoBehaviour CsScript;

	public void DrawSmoothCurve(Vector3[] pointarray,float maxThreshold,float width){

		List<Vector3> points = ConvertArraytoList(pointarray);
		OrderedDictionary Reducedpoints;
		int[] SortedIndices;
		
		Reducedpoints = new OrderedDictionary();
		PointReduction(points,Reducedpoints,1,maxThreshold);
		
		//Sort the Reducedpoints dictionary by their int key.
		SortedIndices = new int[Reducedpoints.Count];
		Reducedpoints.Keys.CopyTo(SortedIndices,0);
		Array.Sort(SortedIndices);
		Debug.Log("SORT" + SortedIndices.Length);
		
		//Call Smoothing Function
		DrawCatmullRomSpline(points,SortedIndices,width);
	}

	public List<Vector3> ConvertArraytoList(Vector3[] abc){
		
		List<Vector3> ListofPoints = new List<Vector3>(abc);
		return ListofPoints ;
	}

	void Start () {

		col1= new Color(0,0,0);
		distanceThreshold  = 0.25f;
	//	drawing = false;	
	}
	
	void PointReduction(List<Vector3> PointsinCurve,OrderedDictionary Reducedpoints,int last,float maxthresh){
		int index1,index2;
		if(Reducedpoints.Count==0){			
			/* Adds 1st and last point of curve in reduced list, calls Findfarthestpoint with the reduced points as argument 
		and that returns an index = the farthest point's index in the pointList.If it is a valid index, add that point to reducedpoints list.*/
			Reducedpoints.Add(0,PointsinCurve[0]);
			Reducedpoints.Add(PointsinCurve.Count-1,PointsinCurve[PointsinCurve.Count-1]);
			index1=FindFarthestPoint((Vector3)Reducedpoints[0],(Vector3)Reducedpoints[Reducedpoints.Count-1],PointsinCurve,1,PointsinCurve.Count,maxthresh);
				if(index1!=0){
					Reducedpoints.Add(index1,PointsinCurve[index1]);
				}
				else {Debug.Log ("ERROR");}
			}
		else {
			/*if reducedlist is not empty,find the point that is marked as last(which has to evaluated)from the reducedpoints dictionary,store the point's index 
		 * as per its location in the pointsInCurve in keynum variable.Then in the reducedpoints dictionary, find the point whose curve location index is just less than the point 
		 * being evaluated.Store that in lowerbound. Similarly point with curve loc index just greater is stored in upperbound. Then farthest point to be calculated between the 
		 * keynum point and upperbound/lowerbound.*/
			int keynum = 0;
			int lowerbound=0;
			int upperbound = PointsinCurve.Count-1;
			foreach(DictionaryEntry de in Reducedpoints){
				if(de.Value == Reducedpoints[last]){
					keynum = (int)de.Key;
					break;
				}
			}	
			foreach(DictionaryEntry de in Reducedpoints){
				if(keynum>(int)de.Key && lowerbound<(int)de.Key){
					lowerbound=(int)de.Key;
				}
				else if(keynum<(int)de.Key && upperbound>(int)de.Key){
					upperbound =(int)de.Key; 
				}
			}
			index1=FindFarthestPoint(PointsinCurve[keynum],PointsinCurve[lowerbound],PointsinCurve,lowerbound,keynum,maxthresh);
			if(index1!=0){
				Reducedpoints.Add(index1,PointsinCurve[index1]);
			}
			index2=FindFarthestPoint(PointsinCurve[upperbound],PointsinCurve[keynum],PointsinCurve,keynum,upperbound,maxthresh);
			if(index2!=0){
				Reducedpoints.Add(index2,PointsinCurve[index2]);
			}
			
		}
		if(last<Reducedpoints.Count-1){
			last++;
			PointReduction(PointsinCurve,Reducedpoints,last,maxthresh);
		}
	}
	
	int FindFarthestPoint(Vector3 endpt1,Vector3 endpt2,List<Vector3> list,int i,int j,float Threshold){
		Vector3 segment = endpt1-endpt2;
		int index=0;
		float maxdist=0;
		float dist=0;
		for(int a = i; a<j;a++){
			dist = (Vector3.Cross(segment,(endpt1-list[a]))/segment.magnitude).magnitude;
			if(dist>maxdist){
				maxdist = dist;
				if(maxdist>Threshold){
					index = a;	
				}
			}
		}
		return index;
	}

	void DrawFittedCurve(List<Vector3> PointsinCurve,int[] SortedIndices){
		int numpoints = 4;
		Vector3 p0 = new Vector3();
		Vector3 p1 = new Vector3();
		int l= SortedIndices.Length;
		for(int j=0;j<l;j++){
			GameObject SharpCurve = new GameObject();
			SharpCurve.AddComponent<LineRenderer>();
			LineRenderer LRnew = SharpCurve.GetComponent<LineRenderer>();
			LRnew.material = new Material (Shader.Find("VertexLit"));
			LRnew.material.SetColor("_Emission", new Color(1,0,0));
			LRnew.SetWidth(0.05f, 0.05f);
			LRnew.SetVertexCount(numpoints);
			Vector3 position = new Vector3();
			float weight=0;
			p0 = PointsinCurve[SortedIndices[j]];
			if(j!=l-1){
			p1 = PointsinCurve[SortedIndices[j+1]];
			}

			else {break;}
			Debug.Log("P0 P1 "+ p0+p1);
			for(int i = 0; i < numpoints; i++) 
			{
				weight =(float) i /(numpoints);
				position = (1-weight)*p0 + weight*p1 ;
				LRnew.SetPosition(i,position);
			}

		}
	}

	void DrawCatmullRomSpline(List<Vector3> OriginalPointList,int[] PointIndices,float width){
		
		int numpoints = OriginalPointList.Count/(PointIndices.Length);
		Debug.Log("numpoints = "+numpoints);
		Vector3 p0,p1,m0,m1;
		int l= PointIndices.Length;
		GameObject correctline = new GameObject();
		correctline.AddComponent<LineRenderer>();
		LineRenderer LRnew = correctline.GetComponent<LineRenderer>();
		LRnew.material= new Material(Shader.Find("Particles/Additive"));
		//	LRnew.material = new Material (Shader.Find("VertexLit"));
		LRnew.material.SetColor("_Emission", col1);
		LRnew.SetWidth(width, width);
		LRnew.SetVertexCount(numpoints * (PointIndices.Length - 1));

		for(int j = 0;j<l-1;j++){
			
			p0 = OriginalPointList[PointIndices[j]];
			p1 = OriginalPointList[PointIndices[j+1]];
	
			if(j>0){
				m0 = 0.5f*(OriginalPointList[PointIndices[j+1]]-OriginalPointList[PointIndices[j-1]]);
			}
			else {
				m0 = OriginalPointList[PointIndices[j+1]]-OriginalPointList[PointIndices[j]];
			}
			if(j < l-2){
				m1 = 0.5f* (OriginalPointList[PointIndices[j+2]]-OriginalPointList[PointIndices[j]]);
			}
			else{
				m1= OriginalPointList[PointIndices[j+1]]-OriginalPointList[PointIndices[j]];
			}
			
			Vector3 Position;
			float t;
			float pointStep = 1/(float)numpoints;
			if(j==l-2){
				pointStep = 1/(float)(numpoints-1) ;
			}
			for(int i =0;i<numpoints;i++){
				t = i*pointStep;
				Position = (2.0f*t*t*t - 3.0f*t*t + 1.0f) * p0  
					+ (t*t*t - 2.0f*t*t + t) * m0
						+ (-2.0f*t*t*t + 3.0f*t*t) * p1
						+ (t*t*t - t*t) * m1;
				
				LRnew.SetPosition(i+j*numpoints,Position);
			}
		}	
	}
	

/*	public bool UserDrawsCurve(List<Vector3> pointList,bool drawing,float distanceThreshold,LineRenderer LR,RaycastHit hit,GameObject LRobj){

		if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 1000)) {
		
			// create new gameObject with lineRenderer
			LRobj = new GameObject("lineObj");
			LRobj.AddComponent<LineRenderer>();
			LR = LRobj.GetComponent<LineRenderer>();
			LR.useWorldSpace = true;
			LR.material = new Material (Shader.Find("VertexLit"));
			LR.material.SetColor("_Emission", lineColor);
			LR.SetWidth(0.05f, 0.05f);
			
			// set position of lineRenderer points
			LR.SetPosition(0, hit.point);   
			LR.SetPosition(1, hit.point);
			
			// add the two points to the array
			pointList = new List<Vector3>();
			pointList.Add(hit.point);  
			pointList.Add(hit.point);     
			drawing = true;

		}
		else {
			drawing = false;
		}
		if (drawing) {
			
			if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 1000)) {
				
				Debug.DrawLine(transform.position,hit.point ) ;
				
				// check if point is past the distance threshold from the previous point in the array
				if (Vector3.Distance( hit.point, pointList[pointList.Count-2]) > distanceThreshold ) {
					// add a new point
					LR.SetVertexCount(pointList.Count+1);
					// add a new poinnt to the array
					pointList.Add(hit.point); 
					// update position before redraw
					LR.SetPosition(pointList.Count-1, hit.point);		
				}
			}
		}	
		return drawing;
	}*/
	
	void Update () {
	
	} 
	
}

