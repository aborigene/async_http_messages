package com.dynatrace.sdk_lab.pix_processor.model;


import java.io.Serializable;
import java.sql.Date;
import javax.persistence.Column;

import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
//import org.springframework.data.relational.core.mapping.Column;

@Entity
public class PixOperation implements Serializable{
    private static final long serialVersionUID = 300002228479017363L;

    
    public long Date;
    public int Value;
    @Column(name = "EndToEndID")
    public String EndToEndID;
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    //@Column()
    private Long id;
    
    
    public PixOperation(long Date, int Value, String EndToEndID){
        this.Date = Date;
        this.Value = Value;
        this.EndToEndID = EndToEndID;
    }
    
    public PixOperation(){ }
    
    public long getDate(){
        return this.Date;
    }
    
    public int getValue(){
        return this.Value;
    }
    
    public String getEndToEndID(){
        return this.EndToEndID;
    }

    public Long getId() {
        return id;
    }

    public void setId(Long id) {
        this.id = id;
    }
    
}
