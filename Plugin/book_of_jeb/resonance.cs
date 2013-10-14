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
		double[] harmonics = new double[7];
		uint current_eccent_index = 0;
		uint current_harm_index = 0;
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
		
		int[] ratio(double x, double y) {
			int[] ratio = new int[2];
			for(int i = 1; i < 10; i++) {
				for(int j = 1; j < 10; j++) {
					if(Math.Abs((x*i)-(y*j)) < 0.01) {
						ratio[0] = i;
						ratio[1] = j;
						return ratio;
					}
				}
			}
			ratio[0] = 0;
			ratio[1] = 0;
			return ratio;
		}
		
		public override void OnStart(StartState state)
		{
			for(double i = 0; i <= 0.9; i += 0.1) {
				eccentricities[Convert.ToInt32(i*10.0)] = i.ToString();
			}
			eccentricities[10] = "Current";
			harmonics[0] = 3;
			harmonics[1] = 2;
			harmonics[2] = 1.5;
			harmonics[3] = 1;
			harmonics[4] = (2.0/3.0);
			harmonics[5] = 0.5;
			harmonics[6] = (1.0/3.0);
		}
		
		[KSPEvent(guiActive = true, guiName = "Enable planet detector", active = true)]
		public void Enable()
		{
			Enabled = true;
			windowStyle = new GUIStyle(HighLogic.Skin.window);
			orbiting_body = this.vessel.mainBody;
		}
		
		[KSPEvent(guiActive = true, guiName = "Disable planet detector", active = false)]
		public void Disable() {
			Enabled = false;
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
			bool eccent_left = GUILayout.Button("<");
			GUILayout.Label("Eccentricity: "+eccentricities[current_eccent_index]);
			bool eccent_right = GUILayout.Button(">");
			
			if (eccent_left) {
				if(current_eccent_index != 0) {
					current_eccent_index--;
				} else {
					current_eccent_index = 10;
				}
			}
			if (eccent_right) {
				if(current_eccent_index != 10) {
					current_eccent_index++;
				} else {
					current_eccent_index = 0;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			bool harm_left = GUILayout.Button("<");
			GUILayout.Label("Harmonic (days:orbit): "+ratio(harmonics[current_harm_index],1)[0].ToString()+":"+ratio(harmonics[current_harm_index], 1)[1].ToString());
			bool harm_right = GUILayout.Button(">");
			
			if (harm_left) {
				if(current_harm_index != 0) {
					current_harm_index--;
				} else {
					current_harm_index = 6;
				}
			}
			
			if (harm_right) {
				if(current_harm_index != 6) {
					current_harm_index++;
				} else {
					current_harm_index = 0;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginVertical();
			
			double desired_eccentricity = 0;
			if(current_eccent_index == 10) {
				desired_eccentricity = eccentricity;
			} else {
				desired_eccentricity = Convert.ToDouble(eccentricities[current_eccent_index]);
			}
			
			double cur_eccentricity = ((this.vessel.orbit.ApA-this.vessel.orbit.PeA)/2.0)/(this.vessel.orbit.PeA+((this.vessel.orbit.ApA-this.vessel.orbit.PeA)/2.0));
			
			if(started == true || new_orbiting_body.name != orbiting_body.name || eccent_left || eccent_right || harm_left || harm_right || Math.Abs(cur_eccentricity-eccentricity) > 0.05) {
				double day = new_orbiting_body.rotationPeriod/harmonics[current_harm_index];
				grav_day = -(new_orbiting_body.gravParameter*(Math.Pow(day, 2))/(4*Math.Pow(Math.PI, 2)));
			
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

