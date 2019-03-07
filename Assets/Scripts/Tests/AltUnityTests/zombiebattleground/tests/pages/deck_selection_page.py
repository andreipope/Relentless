from base import CZBTests
class Deck_Selection_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.deck_selection_page=self.deck_mode_selection_page()
        self.back_button=self.get_back_button()
        self.battle_button=self.get_battle_button()
        self.left_arrow_button=self.get_left_arrow_button()
        self.right_arrow_button=self.get_right_arrow_button()
        self.army_button=self.get_army_button()
        self.deck_container_panel=self.get_deck_container_panel()
        self.edit_button=self.get_edit_button()
        self.delete_button=self.get_delete_button()
        
    
    def deck_mode_selection_page(self):
        return self.altdriver.wait_for_element('HordeSelectionPage(Clone)')
    def get_back_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Button_Back')
    def get_battle_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Button_Battle')
    def get_left_arrow_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Button_LeftArrow')
    def get_right_arrow_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Button_RightArrow')
    def get_army_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Button_Army')
    def get_deck_container_panel(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Panel_DecksContainer')
    def get_edit_button(self):
        return self.altdriver.wait_for_element(self.deck_container_panel.name+'/SelectionMask/Selection/Panel_SelectedBlock/Panel_SelectedHordeObjects/Button_Edit')
    def get_delete_button(self):
        return self.altdriver.wait_for_element(self.deck_container_panel.name+'/SelectionMask/Selection/Panel_SelectedBlock/Panel_SelectedHordeObjects/Button_Delete')
    

    def start_match(self):
        self.button_pressed(self.battle_button)
    def go_to_army_page(self):
        self.button_pressed(self.army_button)
    def go_back_to_menu(self):
        self.button_pressed(self.back_button)

    def press_right_arrow(self):
        self.button_pressed(self.right_arrow_button)
    def press_left_arrow(self):
        self.button_pressed(self.left_arrow_button)
    def press_edit_button(self):
        self.button_pressed(self.edit_button)
    def press_delete_button(self):
        self.button_pressed(self.delete_button)

    def get_selected_deck(self):
        decks_present=self.altdriver.find_elements(self.deck_container_panel.name+'/Group/Item_HordeSelectionObject(Clone)/Button_Select',enabled=False)
        for button in decks_present:
            print('button enabled '+button.enabled)
            if button.enabled=='False':
                return self.altdriver.wait_for_element('id('+button.id+')/..',enabled=False)
        return None

    def get_deck_name(self,deck):
        print(deck.name+"  "+ deck.id)
        text_element=self.altdriver.find_element('id('+deck.id+')/Panel_Description/Text_Description')
        return self.read_tmp_UGUI_text(text_element)
    def get_deck_number_of_cards(self,deck):
        text_element=self.altdriver.find_element('id('+deck.id+')/Panel_DeckFillInfo/Text_CardsCount')
        return self.read_tmp_UGUI_text(text_element).split('/')[0]

    def select_last_deck(self):
        selected_deck=self.get_selected_deck()
        while True:
            self.press_right_arrow()
            new_selected_deck=self.get_selected_deck()
            if selected_deck.id==new_selected_deck.id:
                break
            selected_deck=new_selected_deck
    def select_first_deck(self):
        selected_deck=self.get_selected_deck()
        while True:
            self.press_left_arrow()
            new_selected_deck=self.get_selected_deck()
            if selected_deck.id==new_selected_deck.id:
                break
            selected_deck=new_selected_deck
    def select_deck(self,deck_name):
        selected_deck=self.get_selected_deck()
        print(selected_deck.name+" jlakdjla")
        selected_deck_name=self.get_deck_name(selected_deck)
        if(selected_deck_name==deck_name):
            return selected_deck
        self.select_first_deck()

        selected_deck=self.get_selected_deck
        selected_deck_name=self.get_deck_name(selected_deck)
        if(selected_deck_name==deck_name):
            return selected_deck

        while True:
            self.press_right_arrow()
            new_selected_deck=self.get_selected_deck()
            new_selected_deck_name=self.get_deck_name(new_selected_deck)
            if new_selected_deck_name==deck_name:
                return new_selected_deck
        
        return None


    def create_new_deck(self):
        self.select_first_deck()
        self.altdriver.find_element('Item_HordeSelectionNewHordeLeft/Image_BaackgroundGeneral').mobile_tap()
    def create_new_deck_tutorial(self):
        self.select_last_deck()
        self.altdriver.find_element('Item_HordeSelectionNewHorde/Image_BaackgroundGeneral').mobile_tap()



