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
		double[] resonances = new double[7];
		uint current_eccent_index = 0;
		uint current_reson_index = 0;
		CelestialBody orbiting_body;
		
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
			resonances[0] = 3;
			resonances[1] = 2;
			resonances[2] = 1.5;
			resonances[3] = 1;
			resonances[4] = (2.0/3.0);
			resonances[5] = 0.5;
			resonances[6] = (1.0/3.0);
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
			bool reson_left = GUILayout.Button("<");
			GUILayout.Label("Resonance (days:orbit): "+ratio(resonances[current_reson_index],1)[0].ToString()+":"+ratio(resonances[current_reson_index], 1)[1].ToString());
			bool reson_right = GUILayout.Button(">");
			
			if (reson_left) {
				if(current_reson_index != 0) {
					current_reson_index--;
				} else {
					current_reson_index = 6;
				}
			}
			
			if (reson_right) {
			if(current_reson_index != 6) {
				current_reson_index++;
			} else {
				current_reson_index = 0;
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
			
			double cur_eccentricity = this.vessel.orbit.eccentricity;
			
			if(started == true || new_orbiting_body.name != orbiting_body.name || eccent_left || eccent_right || reson_left || reson_right || Math.Abs(cur_eccentricity-eccentricity) > 0.05) {
				double period = new_orbiting_body.rotationPeriod/resonances[current_reson_index];
				double semimajor = Math.Pow((period*period*new_orbiting_body.gravParameter)/(4*Math.PI*Math.PI), 1.0/3.0);
				geo_orbit_ap = semimajor*(1+desired_eccentricity);
				geo_orbit_pe = semimajor*(1-desired_eccentricity);
				
				orbiting_body = new_orbiting_body;
				eccentricity = cur_eccentricity;
			}
		
			if(geo_orbit_ap > orbiting_body.sphereOfInfluence || geo_orbit_pe < orbiting_body.Radius) {
				GUILayout.Label("Orbit of this eccentricity and resonance is unavailable above this body.");				
			} else {
				GUILayout.Label(ratio(resonances[current_reson_index],1)+" orbit apoapsis "+Math.Round((geo_orbit_ap-orbiting_body.Radius)/1000).ToString()+"km above 'sea level'");
				GUILayout.Label(ratio(resonances[current_reson_index],1)+" orbit periapsis "+Math.Round((geo_orbit_pe-orbiting_body.Radius)/1000).ToString()+"km above 'sea level'");
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
