using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneTest : MonoBehaviour {
    // Start is called before the first frame update
    public class Person {
        public string name;
        public Person mother;
        public Person(string name, Person mother) {
            this.name = name;
            this.mother = mother;
        }

        public Person ShallowCopy() {
            return (Person) this.MemberwiseClone();
        }
    }
    private void Start() {
        Person linda = new Person("Linda", null);
        Person alice = new Person("Alice", null);
        Person johnny = new Person("Johnny", linda);

        Person johnnyCopy = johnny.ShallowCopy();
        Debug.Log("Before:\n Johnny name: " + johnny.name + " Johnny mom name: " + johnny.mother.name +
                  "Johnny copy name: "     + johnnyCopy.name + " Johnny copy mom name: " + johnnyCopy.mother.name);
        
        johnny.mother.name = "Wanda";
        
        Debug.Log("After:\n Johnny name: " + johnny.name + " Johnny mom name: " + johnny.mother.name +
                  "Johnny copy name: " + johnnyCopy.name + " Johnny copy mom name: " + johnnyCopy.mother.name);
    }
     
    // Update is called once per frame
    private void Update() {
        
    }
}

