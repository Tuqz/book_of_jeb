using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace book_of_jeb
{
	public class resonance : PartModule
	{
		public bool Enabled;
		private static GUIStyle windowStyle = null;
		private static Rect windowPosition = new Rect(0, 0, 320, 320);
		string[] eccentricities = new string[11];
		uint current_index = 0;
		CelestialBody orbiting_body;
		
		double grav_day = 0;
			
		double c = 0;
			
		double R = 0;
		double Q = 0;
		
		double S = 0;
		double T = 0;
		double geo_orbit_ap = 0;
		double geo_orbit_pe = 0;
		
		double eccentricity = 0;
		
		bool started = true;
		
		int cmn_fctr(int x, int y) {
			int i = 1;
			bool fctr_fnd = false;
			while(!fctr_fnd && i < Math.Min(x, y)) {
				i++;
				if((x%i) == 0 && (y%i) == 0) {
					fctr_fnd = true;
				}
			}
			if(fctr_fnd) {
				return i;
			}
			return 0;
		}
		
		public override void OnStart(StartState state)
		{
			print ("The part has been loaded");
			for(double i = 0; i <= 0.9; i += 0.1) {
				eccentricities[Convert.ToInt32(i*10.0)] = i.ToString();
			}
			eccentricities[10] = "Current";
		}
		
		[KSPEvent(guiActive = true, guiName = "Enable planet detector", active = true)]
		public void Enable()
		{
			Enabled = true;
			windowStyle = new GUIStyle(HighLogic.Skin.window);
			print ("On");
			orbiting_body = this.vessel.mainBody;
		}
		
		[KSPEvent(guiActive = true, guiName = "Disable planet detector", active = false)]
		public void Disable() {
			Enabled = false;
			print ("Off");
		}
		
		public override void OnUpdate() {
			print (this.vessel.mainBody.name);
			RenderingManager.AddToPostDrawQueue(0, OnDraw);
		}
		
		public void OnDraw() {
			if(Enabled) {
				windowPosition = GUI.Window (1234, windowPosition, OnWindow, "Resonances", windowStyle);
			}
		}
		
		public void OnWindow(int windowID) {
			CelestialBody new_orbiting_body = this.vessel.mainBody;
				
			GUILayout.BeginVertical();
			GUILayout.Label(orbiting_body.name);
			GUILayout.EndVertical();
			GUILayout.BeginHorizontal();
			bool left = GUILayout.Button("<");
			GUILayout.Label("Eccentricity: "+eccentricities[current_index]);
			bool right = GUILayout.Button(">");
			
			if (left) {
				if(current_index != 0) {
					current_index--;
				} else {
					current_index = 10;
				}
			}
			if (right) {
				if(current_index != 10) {
					current_index++;
				} else {
					current_index = 0;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginVertical();
			
			double desired_eccentricity = 0;
			if(current_index == 10) {
				desired_eccentricity = eccentricity;
			} else {
				desired_eccentricity = Convert.ToDouble(eccentricities[current_index]);
			}
			
			double cur_eccentricity = ((this.vessel.orbit.ApA-this.vessel.orbit.PeA)/2.0)/(this.vessel.orbit.PeA+((this.vessel.orbit.ApA-this.vessel.orbit.PeA)/2.0));
			
			if(started == true || new_orbiting_body.name != orbiting_body.name || left || right || Math.Abs(cur_eccentricity-eccentricity) > 0.05) {
				grav_day = -(orbiting_body.gravParameter*(Math.Pow(orbiting_body.rotationPeriod, 2))/(4*Math.Pow(Math.PI, 2)));
			
				c = (desired_eccentricity * desired_eccentricity)-2*desired_eccentricity; //Following is from Cardano's formula for cubics, see http://www.proofwiki.org/wiki/Cardano%27s_Formula
			
				R = (-grav_day)/2.0;
				Q = c/3.0;
		
				S = Math.Pow(R + Math.Sqrt(Math.Pow(Q, 3)+Math.Pow(R, 2)), 1.0/3.0);
				T = Math.Pow(R - Math.Sqrt(Math.Pow(Q, 3)+Math.Pow(R, 2)), 1.0/3.0);
				geo_orbit_ap = (S+T)+((S+T)*desired_eccentricity);
				geo_orbit_pe = (S+T)-((S+T)*desired_eccentricity);
				
				orbiting_body = new_orbiting_body;
				eccentricity = ((this.vessel.orbit.ApA-this.vessel.orbit.PeA)/2.0)/(this.vessel.orbit.PeA+((this.vessel.orbit.ApA-this.vessel.orbit.PeA)/2.0));
			}
			
			if(geo_orbit_ap > orbiting_body.sphereOfInfluence || geo_orbit_pe < orbiting_body.Radius) {
				GUILayout.Label("1:1 orbit of this eccentricity unavailable above this body.");
			} else {
				GUILayout.Label("1:1 orbit apoapsis "+Math.Round((geo_orbit_ap-orbiting_body.Radius)/1000).ToString()+"km above 'sea level'");
				GUILayout.Label("1:1 orbit periapsis "+Math.Round((geo_orbit_pe-orbiting_body.Radius)/1000).ToString()+"km above 'sea level'");
				GUILayout.Label("Current orbital apoapsis "+Math.Round(this.vessel.orbit.ApA/1000.0)+"km above 'sea level'");
				GUILayout.Label("Current orbital periapsis "+Math.Round(this.vessel.orbit.PeA/1000.0) +"km above 'sea level'");
				GUILayout.Label("Current orbital eccentricity "+((Math.Round(eccentricity*100))/100).ToString());
			}
			GUILayout.EndVertical();
			
			GUILayout.BeginHorizontal();
			Enabled = GUILayout.Toggle(Enabled, "On");
			GUILayout.EndHorizontal();
			GUI.DragWindow();
			started = false;
		}
	}
}

