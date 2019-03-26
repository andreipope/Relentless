from .base import CZBTests
import time
class Battle_Mode_Selection_Popup_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.battle_mode_selection_popup_page=self.altdriver.wait_for_element('GameModePopup(Clone)')
        self.back_button=self.altdriver.wait_for_element(self.battle_mode_selection_popup_page.name+'/Button_Back')
        self.solo_mode_button=self.altdriver.wait_for_element(self.battle_mode_selection_popup_page.name+'/Button_SoloMode')
        self.pvp_mode_button=self.altdriver.wait_for_element(self.battle_mode_selection_popup_page.name+'/Button_PvPMode')
        self.tutorial_button=self.altdriver.wait_for_element(self.battle_mode_selection_popup_page.name+'/Button_Tutorial')
        
    
   
    def press_solo_mode_button(self):
        self.button_pressed(self.solo_mode_button)
    def press_back_button(self):
        self.button_pressed(self.back_button)
    def press_pvp_mode_button(self):
        self.button_pressed(self.pvp_mode_button)
    def press_tutorial_button(self):
        self.button_pressed(self.tutorial_button)
    